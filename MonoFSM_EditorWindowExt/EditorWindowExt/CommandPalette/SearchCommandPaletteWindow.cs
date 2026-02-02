#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using SearchField = UnityEditor.IMGUI.Controls.SearchField;

namespace CommandPalette
{
    /// <summary>
    /// 命令面板 - 使用自訂搜尋引擎進行即時搜尋
    /// 快捷鍵: Cmd+T (Mac) / Ctrl+T (Windows)
    /// 支援 Prefabs, ScriptableObjects, Scenes, MenuItems, Windows
    /// </summary>
    public class SearchCommandPaletteWindow : EditorWindow
    {
        private SearchField _searchField;
        private string _searchString = "";
        private Vector2 _scrollPos;
        private int _selectedIndex = -1;
        private SearchMode _currentMode = SearchMode.Prefabs;
        private static SearchCommandPaletteWindow _instance;

        // 各模式的搜尋結果
        private List<SearchResult<AssetEntry>> _assetResults = new();
        private List<SearchResult<MenuItemEntry>> _menuItemResults = new();
        private List<SearchResult<EditorWindowEntry>> _windowResults = new();

        // 資源快取
        private Dictionary<SearchMode, List<AssetEntry>> _assetCache = new();
        private List<MenuItemEntry> _menuItemCache;
        private List<EditorWindowEntry> _windowCache;

        private const float RowHeight = 22f;
        private const float TabHeight = 25f;
        private const float PathBarHeight = 20f;
        private const string SearchModePrefKey = "CommandPalette_SearchMode";
        private const int MaxResults = 100;

        // 搜尋模式配置 - AssetDatabase 篩選條件
        private static readonly Dictionary<SearchMode, string> AssetDatabaseFilters =
            new()
            {
                { SearchMode.Prefabs, "t:GameObject" },
                { SearchMode.ScriptableObjects, "t:ScriptableObject" },
                { SearchMode.Scenes, "t:SceneAsset" },
            };

        private static readonly Dictionary<SearchMode, string> ModeNames =
            new()
            {
                { SearchMode.Prefabs, "Prefabs" },
                { SearchMode.ScriptableObjects, "ScriptableObjects" },
                { SearchMode.Scenes, "Scenes" },
                { SearchMode.MenuItems, "MenuItems" },
                { SearchMode.Windows, "Windows" },
            };

        [MenuItem("Tools/Search Command Palette %t")]
        public static void OpenWindow()
        {
            if (_instance != null)
            {
                _instance.Close();
                return;
            }

            _instance = CreateInstance<SearchCommandPaletteWindow>();
            _instance.titleContent = new GUIContent("Command Palette");
            _instance.ShowUtility();
            _instance.Focus();

            // 簡單置中
            var rect = new Rect(200, 200, 500, 400);
            _instance.position = rect;
        }

        private void OnEnable()
        {
            _currentMode = (SearchMode)
                EditorPrefs.GetInt(SearchModePrefKey, (int)SearchMode.Prefabs);
            LoadCacheForCurrentMode();
            PerformSearch();
        }

        private void LoadCacheForCurrentMode()
        {
            switch (_currentMode)
            {
                case SearchMode.Prefabs:
                case SearchMode.ScriptableObjects:
                case SearchMode.Scenes:
                    if (!_assetCache.ContainsKey(_currentMode))
                    {
                        _assetCache[_currentMode] = LoadAssetsForMode(_currentMode);
                    }
                    break;

                case SearchMode.MenuItems:
                    _menuItemCache ??= SearchCommandPaletteCacheHelper.CollectAllMenuItems();
                    break;

                case SearchMode.Windows:
                    _windowCache ??= EditorWindowSearchHelper.GetAllEditorWindowTypes();
                    break;
            }
        }

        private List<AssetEntry> LoadAssetsForMode(SearchMode mode)
        {
            var assets = new List<AssetEntry>();

            if (!AssetDatabaseFilters.TryGetValue(mode, out var filter))
                return assets;

            var guids = AssetDatabase.FindAssets(filter);

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);

                // 只過濾 Unity 官方套件，保留本地 Packages
                if (path.StartsWith("Packages/com.unity."))
                    continue;

                var name = System.IO.Path.GetFileNameWithoutExtension(path);
                // 不載入 asset，只儲存 metadata
                assets.Add(new AssetEntry(name, path, guid));
            }

            return assets;
        }

        private void OnGUI()
        {
            HandleKeyboardInput();
            DrawModeTab();
            DrawSearchField();
            DrawResultsList();
            DrawPathBar();
        }

        private void HandleKeyboardInput()
        {
            if (Event.current.type != EventType.KeyDown)
                return;

            var resultsCount = GetResultsCount();

            switch (Event.current.keyCode)
            {
                case KeyCode.Escape:
                    Close();
                    Event.current.Use();
                    break;

                case KeyCode.Tab:
                    // Tab 永遠切換模式（中文輸入不使用 Tab 選字）
                    CycleModes();
                    Event.current.Use();
                    break;

                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    // 正在編輯文字時不攔截 Enter（避免影響中文選字）
                    if (!EditorGUIUtility.editingTextField && _selectedIndex >= 0 && _selectedIndex < resultsCount)
                    {
                        OpenSelectedResult();
                        Event.current.Use();
                    }
                    break;

                case KeyCode.UpArrow:
                    if (resultsCount > 0)
                    {
                        GUIUtility.keyboardControl = 0; // 退出 textfield 編輯狀態
                        _selectedIndex = _selectedIndex <= 0 ? resultsCount - 1 : _selectedIndex - 1;
                        ScrollToSelected();
                        Event.current.Use();
                        Repaint();
                    }
                    break;

                case KeyCode.DownArrow:
                    if (resultsCount > 0)
                    {
                        GUIUtility.keyboardControl = 0; // 退出 textfield 編輯狀態
                        _selectedIndex = _selectedIndex >= resultsCount - 1 ? 0 : _selectedIndex + 1;
                        ScrollToSelected();
                        Event.current.Use();
                        Repaint();
                    }
                    break;
            }
        }

        private void CycleModes()
        {
            var modes = Enum.GetValues(typeof(SearchMode)).Cast<SearchMode>().ToArray();
            var currentIndex = Array.IndexOf(modes, _currentMode);
            _currentMode = modes[(currentIndex + 1) % modes.Length];
            EditorPrefs.SetInt(SearchModePrefKey, (int)_currentMode);
            LoadCacheForCurrentMode();
            PerformSearch();
            if (_searchField != null)
                _searchField.SetFocus();
            Repaint();
        }

        private void DrawModeTab()
        {
            var tabRect = new Rect(0, 0, position.width, TabHeight);
            var modes = Enum.GetValues(typeof(SearchMode)).Cast<SearchMode>().ToArray();
            var tabWidth = position.width / modes.Length;

            EditorGUI.DrawRect(tabRect, new Color(0.2f, 0.2f, 0.2f, 1f));

            var originalColor = GUI.color;
            for (int i = 0; i < modes.Length; i++)
            {
                var mode = modes[i];
                var tabButtonRect = new Rect(i * tabWidth, 0, tabWidth, TabHeight);

                GUI.color =
                    _currentMode == mode
                        ? new Color(0.4f, 0.6f, 1f, 1f)
                        : new Color(0.7f, 0.7f, 0.7f, 1f);

                if (GUI.Button(tabButtonRect, ModeNames[mode], EditorStyles.toolbarButton))
                {
                    if (_currentMode != mode)
                    {
                        _currentMode = mode;
                        EditorPrefs.SetInt(SearchModePrefKey, (int)_currentMode);
                        LoadCacheForCurrentMode();
                        PerformSearch();
                        if (_searchField != null)
                            _searchField.SetFocus();
                        Repaint();
                    }
                }
            }
            GUI.color = originalColor;
        }

        private void DrawSearchField()
        {
            if (_searchField == null)
            {
                _searchField = new SearchField();
                _searchField.SetFocus();
            }

            var searchRect = new Rect(5, TabHeight + 5, position.width - 10, 18);
            var newSearchString = _searchField.OnGUI(searchRect, _searchString);

            if (newSearchString != _searchString)
            {
                _searchString = newSearchString;
                PerformSearch();
            }
        }

        private void PerformSearch()
        {
            _selectedIndex = -1;

            switch (_currentMode)
            {
                case SearchMode.Prefabs:
                case SearchMode.ScriptableObjects:
                case SearchMode.Scenes:
                    PerformAssetSearch();
                    break;

                case SearchMode.MenuItems:
                    PerformMenuItemSearch();
                    break;

                case SearchMode.Windows:
                    PerformWindowSearch();
                    break;
            }

            Repaint();
        }

        private void PerformAssetSearch()
        {
            _assetResults.Clear();

            if (!_assetCache.TryGetValue(_currentMode, out var assets) || assets == null)
            {
                LoadCacheForCurrentMode();
                assets = _assetCache.GetValueOrDefault(_currentMode);
            }

            if (assets == null || assets.Count == 0)
                return;

            _assetResults = SearchEngine.Search(_searchString, assets, MaxResults);

            if (_assetResults.Count > 0)
                _selectedIndex = 0;
        }

        private void PerformMenuItemSearch()
        {
            _menuItemResults.Clear();

            if (_menuItemCache == null || _menuItemCache.Count == 0)
            {
                LoadCacheForCurrentMode();
            }

            if (_menuItemCache == null || _menuItemCache.Count == 0)
                return;

            _menuItemResults = SearchEngine.Search(_searchString, _menuItemCache, MaxResults);

            if (_menuItemResults.Count > 0)
                _selectedIndex = 0;
        }

        private void PerformWindowSearch()
        {
            _windowResults.Clear();

            if (_windowCache == null || _windowCache.Count == 0)
            {
                LoadCacheForCurrentMode();
            }

            if (_windowCache == null || _windowCache.Count == 0)
                return;

            _windowResults = SearchEngine.Search(_searchString, _windowCache, MaxResults);

            if (_windowResults.Count > 0)
                _selectedIndex = 0;
        }

        private int GetResultsCount()
        {
            return _currentMode switch
            {
                SearchMode.Prefabs or SearchMode.ScriptableObjects or SearchMode.Scenes => _assetResults.Count,
                SearchMode.MenuItems => _menuItemResults.Count,
                SearchMode.Windows => _windowResults.Count,
                _ => 0
            };
        }

        private void ScrollToSelected()
        {
            if (_selectedIndex < 0)
                return;

            var listStartY = TabHeight + 28;
            var listHeight = position.height - listStartY - PathBarHeight;

            var itemTop = _selectedIndex * RowHeight;
            var itemBottom = itemTop + RowHeight;

            // 如果選中項目在可見區域上方，滾動到該項目
            if (itemTop < _scrollPos.y)
            {
                _scrollPos.y = itemTop;
            }
            // 如果選中項目在可見區域下方，滾動使其可見
            else if (itemBottom > _scrollPos.y + listHeight)
            {
                _scrollPos.y = itemBottom - listHeight;
            }
        }

        private void DrawResultsList()
        {
            var resultsCount = GetResultsCount();
            var listStartY = TabHeight + 28;
            var listHeight = position.height - listStartY - PathBarHeight;
            var listRect = new Rect(0, listStartY, position.width, listHeight);
            var contentRect = new Rect(0, 0, position.width - 20, resultsCount * RowHeight);

            _scrollPos = GUI.BeginScrollView(listRect, _scrollPos, contentRect);

            var isEditingTextField = EditorGUIUtility.editingTextField;

            for (var i = 0; i < resultsCount; i++)
            {
                var rect = new Rect(0, i * RowHeight, position.width - 20, RowHeight);

                // 選中狀態背景（編輯文字時顯示灰色，表示 Enter 不會觸發選取）
                if (i == _selectedIndex)
                {
                    var selectedColor = isEditingTextField
                        ? new Color(0.4f, 0.4f, 0.4f, 0.6f)  // 灰色：編輯中
                        : new Color(0.3f, 0.5f, 0.85f, 0.8f); // 藍色：可選取
                    EditorGUI.DrawRect(rect, selectedColor);
                }
                else if (rect.Contains(Event.current.mousePosition))
                    EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.3f));

                // 圖標與標題
                var iconRect = new Rect(rect.x + 5, rect.y + 3, 16, 16);
                var nameRect = new Rect(rect.x + 25, rect.y, rect.width - 30, rect.height);

                DrawResultItem(i, iconRect, nameRect);

                // 滑鼠點擊
                if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
                {
                    _selectedIndex = i;
                    if (Event.current.clickCount == 2)
                        OpenSelectedResult();
                    Event.current.Use();
                    Repaint();
                }
            }

            GUI.EndScrollView();
        }

        private void DrawResultItem(int index, Rect iconRect, Rect nameRect)
        {
            switch (_currentMode)
            {
                case SearchMode.Prefabs:
                case SearchMode.ScriptableObjects:
                case SearchMode.Scenes:
                    if (index < _assetResults.Count)
                    {
                        var entry = _assetResults[index].Item;
                        var icon = entry.icon;
                        if (icon != null)
                            GUI.DrawTexture(iconRect, icon);
                        GUI.Label(nameRect, entry.name);
                    }
                    break;

                case SearchMode.MenuItems:
                    if (index < _menuItemResults.Count)
                    {
                        var entry = _menuItemResults[index].Item;
                        // MenuItem 使用預設圖標
                        var icon = EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow").image;
                        if (icon != null)
                            GUI.DrawTexture(iconRect, icon);
                        GUI.Label(nameRect, $"{entry.displayName}  ({entry.category})");
                    }
                    break;

                case SearchMode.Windows:
                    if (index < _windowResults.Count)
                    {
                        var entry = _windowResults[index].Item;
                        var icon = EditorGUIUtility.IconContent("d_UnityEditor.SceneHierarchyWindow").image;
                        if (icon != null)
                            GUI.DrawTexture(iconRect, icon);
                        GUI.Label(nameRect, $"{entry.DisplayName}  ({entry.Category})");
                    }
                    break;
            }
        }

        private void DrawPathBar()
        {
            var pathBarRect = new Rect(0, position.height - PathBarHeight, position.width, PathBarHeight);
            EditorGUI.DrawRect(pathBarRect, new Color(0.15f, 0.15f, 0.15f, 1f));

            var path = GetSelectedItemPath();
            if (!string.IsNullOrEmpty(path))
            {
                var labelRect = new Rect(5, position.height - PathBarHeight + 2, position.width - 10, PathBarHeight - 4);
                var style = new GUIStyle(EditorStyles.miniLabel)
                {
                    normal = { textColor = new Color(0.6f, 0.6f, 0.6f, 1f) },
                    alignment = TextAnchor.MiddleLeft
                };
                GUI.Label(labelRect, path, style);
            }
        }

        private string GetSelectedItemPath()
        {
            if (_selectedIndex < 0)
                return "";

            switch (_currentMode)
            {
                case SearchMode.Prefabs:
                case SearchMode.ScriptableObjects:
                case SearchMode.Scenes:
                    if (_selectedIndex < _assetResults.Count)
                        return _assetResults[_selectedIndex].Item.path;
                    break;

                case SearchMode.MenuItems:
                    if (_selectedIndex < _menuItemResults.Count)
                        return _menuItemResults[_selectedIndex].Item.menuPath;
                    break;

                case SearchMode.Windows:
                    if (_selectedIndex < _windowResults.Count)
                    {
                        var entry = _windowResults[_selectedIndex].Item;
                        return entry.Type?.FullName ?? "";
                    }
                    break;
            }

            return "";
        }

        private void OpenSelectedResult()
        {
            var resultsCount = GetResultsCount();
            if (_selectedIndex < 0 || _selectedIndex >= resultsCount)
                return;

            switch (_currentMode)
            {
                case SearchMode.Prefabs:
                case SearchMode.ScriptableObjects:
                case SearchMode.Scenes:
                    OpenAssetResult();
                    break;

                case SearchMode.MenuItems:
                    OpenMenuItemResult();
                    break;

                case SearchMode.Windows:
                    OpenWindowResult();
                    break;
            }
        }

        private void OpenAssetResult()
        {
            if (_selectedIndex >= _assetResults.Count)
                return;

            var entry = _assetResults[_selectedIndex].Item;
            var obj = entry.asset;

            if (obj == null)
                return;

            // 特殊處理 Prefab
            if (_currentMode == SearchMode.Prefabs && obj is GameObject)
            {
                if (!string.IsNullOrEmpty(entry.path))
                {
                    try
                    {
                        var prefabStageUtilityType = typeof(EditorSceneManager).Assembly.GetType(
                            "UnityEditor.SceneManagement.PrefabStageUtility"
                        );
                        var openPrefabMethod = prefabStageUtilityType?.GetMethod(
                            "OpenPrefab",
                            BindingFlags.Public | BindingFlags.Static,
                            null,
                            new[] { typeof(string) },
                            null
                        );

                        if (openPrefabMethod != null)
                        {
                            openPrefabMethod.Invoke(null, new object[] { entry.path });
                            Close();
                            return;
                        }
                    }
                    catch
                    {
                        // 回退到 AssetDatabase.OpenAsset
                    }
                }
            }

            AssetDatabase.OpenAsset(obj);
            Close();
        }

        private void OpenMenuItemResult()
        {
            if (_selectedIndex >= _menuItemResults.Count)
                return;

            var entry = _menuItemResults[_selectedIndex].Item;
            entry.Execute();
            Close();
        }

        private void OpenWindowResult()
        {
            if (_selectedIndex >= _windowResults.Count)
                return;

            var entry = _windowResults[_selectedIndex].Item;
            EditorWindowSearchHelper.OpenEditorWindow(entry);
            Close();
        }

        private void OnLostFocus() => Close();

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
    }
}
#endif

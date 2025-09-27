#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Search;
using UnityEngine;
using SearchField = UnityEditor.IMGUI.Controls.SearchField;

namespace CommandPalette
{
    /// <summary>
    /// 命令面板 - 使用 Unity SearchService 進行即時搜尋
    /// 快捷鍵: Cmd+T (Mac) / Ctrl+T (Windows)
    /// 支援 Prefabs, ScriptableObjects, Scenes, MenuItems
    /// </summary>
    public class SearchCommandPaletteWindow : EditorWindow
    {
        private SearchField _searchField;
        private string _searchString = "";
        private Vector2 _scrollPos;
        private List<SearchItem> _searchResults = new();
        private int _selectedIndex = -1;
        private SearchMode _currentMode = SearchMode.Prefabs;
        private static SearchCommandPaletteWindow _instance;

        private const float RowHeight = 22f;
        private const float TabHeight = 25f;
        private const string SearchModePrefKey = "CommandPalette_SearchMode";

        // 搜尋模式配置
        private static readonly Dictionary<SearchMode, string> SearchProviders =
            new()
            {
                { SearchMode.Prefabs, "t:GameObject" },
                { SearchMode.ScriptableObjects, "t:ScriptableObject" },
                { SearchMode.Scenes, "t:SceneAsset" },
                {
                    SearchMode.MenuItems,
                    ""
                } // MenuItem 需要特殊處理
                ,
            };

        private static readonly Dictionary<SearchMode, string> ModeNames =
            new()
            {
                { SearchMode.Prefabs, "Prefabs" },
                { SearchMode.ScriptableObjects, "ScriptableObjects" },
                { SearchMode.Scenes, "Scenes" },
                { SearchMode.MenuItems, "MenuItems" },
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
            PerformSearch(); // 初始載入時顯示該類型的所有資源
        }

        private void OnGUI()
        {
            HandleKeyboardInput();
            DrawModeTab();
            DrawSearchField();
            DrawResultsList();
        }

        private void HandleKeyboardInput()
        {
            if (Event.current.type != EventType.KeyDown)
                return;

            switch (Event.current.keyCode)
            {
                case KeyCode.Escape:
                    Close();
                    Event.current.Use();
                    break;

                case KeyCode.Tab:
                    CycleModes();
                    Event.current.Use();
                    break;

                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    if (_selectedIndex >= 0 && _selectedIndex < _searchResults.Count)
                    {
                        OpenSelectedResult();
                        Event.current.Use();
                    }

                    break;

                case KeyCode.UpArrow:
                    _selectedIndex =
                        _selectedIndex <= 0 ? _searchResults.Count - 1 : _selectedIndex - 1;
                    Event.current.Use();
                    Repaint();
                    break;

                case KeyCode.DownArrow:
                    _selectedIndex =
                        _selectedIndex >= _searchResults.Count - 1 ? 0 : _selectedIndex + 1;
                    Event.current.Use();
                    Repaint();
                    break;
            }
        }

        private void CycleModes()
        {
            var modes = Enum.GetValues(typeof(SearchMode)).Cast<SearchMode>().ToArray();
            var currentIndex = Array.IndexOf(modes, _currentMode);
            _currentMode = modes[(currentIndex + 1) % modes.Length];
            EditorPrefs.SetInt(SearchModePrefKey, (int)_currentMode);
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
            _searchResults.Clear();
            _selectedIndex = -1;

            if (_currentMode == SearchMode.MenuItems)
            {
                // TODO: MenuItem 搜尋實作
                Repaint();
                return;
            }

            var provider = SearchProviders[_currentMode];
            string query;

            if (string.IsNullOrEmpty(_searchString))
            {
                // 空搜尋時顯示該類型的所有資源
                query = provider;
            }
            else
            {
                // 有關鍵字時進行篩選
                query = $"{provider} {_searchString}";
            }

            using var context = SearchService.CreateContext(new[] { "asset" }, query);
            using var results = SearchService.Request(context, SearchFlags.Synchronous);

            _searchResults = results.Take(50).ToList(); // 限制結果數量
            if (_searchResults.Count > 0)
                _selectedIndex = 0;

            Repaint();
        }

        private void DrawResultsList()
        {
            var listStartY = TabHeight + 28;
            var listRect = new Rect(0, listStartY, position.width, position.height - listStartY);
            var contentRect = new Rect(0, 0, position.width - 20, _searchResults.Count * RowHeight);

            _scrollPos = GUI.BeginScrollView(listRect, _scrollPos, contentRect);

            for (var i = 0; i < _searchResults.Count; i++)
            {
                var result = _searchResults[i];
                var rect = new Rect(0, i * RowHeight, position.width - 20, RowHeight);

                // 選中狀態背景
                if (i == _selectedIndex)
                    EditorGUI.DrawRect(rect, new Color(0.3f, 0.5f, 0.85f, 0.8f));
                else if (rect.Contains(Event.current.mousePosition))
                    EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.3f));

                // 圖標
                var iconRect = new Rect(rect.x + 5, rect.y + 3, 16, 16);
                var obj = result.ToObject<UnityEngine.Object>();
                if (obj != null)
                {
                    var thumbnail = AssetPreview.GetMiniThumbnail(obj);
                    if (thumbnail != null)
                        GUI.DrawTexture(iconRect, thumbnail);
                }

                // 標題
                var nameRect = new Rect(rect.x + 25, rect.y, rect.width - 30, rect.height);
                var displayName = result.ToObject<UnityEngine.Object>()?.name ?? result.id;
                GUI.Label(nameRect, displayName);

                // 滑鼠點擊
                if (
                    Event.current.type == EventType.MouseDown
                    && rect.Contains(Event.current.mousePosition)
                )
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

        private void OpenSelectedResult()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _searchResults.Count)
                return;

            var result = _searchResults[_selectedIndex];
            var obj = result.ToObject<UnityEngine.Object>();

            if (obj == null)
                return;

            // 特殊處理 Prefab
            if (_currentMode == SearchMode.Prefabs && obj is GameObject)
            {
                var assetPath = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    // 嘗試用 PrefabStageUtility 打開 Prefab 編輯模式
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
                            openPrefabMethod.Invoke(null, new object[] { assetPath });
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

            // 一般資源打開
            AssetDatabase.OpenAsset(obj);
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

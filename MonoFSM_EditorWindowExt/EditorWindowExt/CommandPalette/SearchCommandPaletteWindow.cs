#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Search;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using SearchField = UnityEditor.IMGUI.Controls.SearchField;

namespace CommandPalette
{
    /// <summary>
    ///     Prefab 搜尋命令面板 - 參考 VTabs 實作
    ///     使用方式：
    ///     1. 按 Cmd+T (Mac) 或 Ctrl+T (Windows) 開啟搜尋視窗
    ///     2. 輸入 prefab 名稱進行模糊搜尋
    ///     3. 使用方向鍵選擇，按 Enter 打開 prefab 編輯模式
    ///     4. 按 Escape 關閉視窗
    ///     功能：
    ///     - 搜尋專案中所有 prefab（包含 package）
    ///     - 支援模糊搜尋和 CamelCase 匹配
    ///     - 鍵盤導航支援
    ///     - 進入 Prefab 編輯模式
    /// </summary>
    public class SearchCommandPaletteWindow : EditorWindow
    {
        private SearchField searchField;
        private string searchString = "";
        private string prevSearchString = "";
        private Vector2 scrollPos;
        private List<AssetEntry> allAssets = new();
        private List<AssetEntry> filteredAssets = new();
        private int selectedIndex = -1;
        private bool isRefreshing = false;
        private SearchMode currentMode = SearchMode.Prefabs;

        private const float ROW_HEIGHT = 22f;
        private const float WINDOW_WIDTH = 400f;
        private const float WINDOW_HEIGHT = 300f;
        private const float TAB_HEIGHT = 25f;

        // EditorPrefs 鍵值
        private const string SEARCH_STRING_PREF_KEY = "CommandPalette_SearchString";
        private const string SEARCH_MODE_PREF_KEY = "CommandPalette_SearchMode";
        private const string WINDOW_WIDTH_PREF_KEY = "CommandPalette_WindowWidth";
        private const string WINDOW_HEIGHT_PREF_KEY = "CommandPalette_WindowHeight";
        private const string WINDOW_X_PREF_KEY = "CommandPalette_WindowX";
        private const string WINDOW_Y_PREF_KEY = "CommandPalette_WindowY";

        // 輔助方法：取得資源類型定義
        private static AssetTypeDefinition GetAssetTypeDefinition(SearchMode mode)
        {
            return SupportedAssetTypes.First(x => x.Mode == mode);
        }

        // 輔助方法：取得快取檔案路徑
        private static string GetCacheFilePath(SearchMode mode)
        {
            var assetType = GetAssetTypeDefinition(mode);
            return Path.Combine(CacheDirectory, assetType.CacheFileName);
        }

        // 效能測量
        private readonly Stopwatch guiStopwatch = new();
        private readonly Stopwatch cacheLoadStopwatch = new();

        // 靜態視窗實例引用
        private static SearchCommandPaletteWindow instance;

        // 視窗位置和尺寸追蹤
        private Rect previousPosition;

        // AssetDatabase 事件監聽
        private static bool isAssetChangeListenerRegistered = false;

        // 檔案快取路徑
        private static readonly string CacheDirectory = Path.Combine("Library", "CommandPalette");

        // 資源類型定義
        private static readonly List<AssetTypeDefinition> SupportedAssetTypes =
            new()
            {
                new AssetTypeDefinition(
                    SearchMode.Prefabs,
                    "Prefabs",
                    ".prefab",
                    "t:Prefab",
                    typeof(GameObject),
                    "PrefabCache.json"
                ),
                new AssetTypeDefinition(
                    SearchMode.ScriptableObjects,
                    "ScriptableObjects",
                    ".asset",
                    "t:ScriptableObject",
                    typeof(ScriptableObject),
                    "ScriptableObjectCache.json"
                ),
                new AssetTypeDefinition(
                    SearchMode.Scenes,
                    "Scenes",
                    ".unity",
                    "t:Scene",
                    typeof(SceneAsset),
                    "SceneCache.json"
                ),
                new AssetTypeDefinition(
                    SearchMode.MenuItems,
                    "MenuItems",
                    "",
                    "",
                    typeof(object),
                    "MenuItemCache.json"
                ),
            };

        // 動態快取字典
        private static readonly Dictionary<SearchMode, Dictionary<string, AssetEntry>> AssetCaches =
            new()
            {
                { SearchMode.Prefabs, new Dictionary<string, AssetEntry>() },
                { SearchMode.ScriptableObjects, new Dictionary<string, AssetEntry>() },
                { SearchMode.Scenes, new Dictionary<string, AssetEntry>() },
            };

        // MenuItem專用快取
        private static List<MenuItemEntry> menuItemCache = new();
        private static bool menuItemCacheValid = false;

        // 動態快取有效性標誌
        private static readonly Dictionary<SearchMode, bool> CacheValidFlags =
            new()
            {
                { SearchMode.Prefabs, false },
                { SearchMode.ScriptableObjects, false },
                { SearchMode.Scenes, false },
            };

        // 動態待處理變更標誌
        private static readonly Dictionary<SearchMode, bool> PendingChangesFlags =
            new()
            {
                { SearchMode.Prefabs, false },
                { SearchMode.ScriptableObjects, false },
                { SearchMode.Scenes, false },
            };

        [MenuItem("Tools/Search Prefab Command Palette %t")]
        public static void OpenWindow()
        {
            // 檢查是否已經有視窗開啟並且有效
            if (instance != null)
            {
                // 如果視窗已開啟，則關閉它
                instance.Close();
                return;
            }

            // 創建新的視窗實例
            instance = CreateInstance<SearchCommandPaletteWindow>();
            instance.titleContent = new GUIContent("Command Palatee");
            instance.ShowUtility();
            instance.Focus();

            // 設置視窗位置和大小 - 從 EditorPrefs 載入或使用預設值
            var savedWidth = EditorPrefs.GetFloat(WINDOW_WIDTH_PREF_KEY, WINDOW_WIDTH);
            var savedHeight = EditorPrefs.GetFloat(WINDOW_HEIGHT_PREF_KEY, WINDOW_HEIGHT);
            var savedX = EditorPrefs.GetFloat(WINDOW_X_PREF_KEY, -1f);
            var savedY = EditorPrefs.GetFloat(WINDOW_Y_PREF_KEY, -1f);

            var rect = new Rect();

            // 如果有保存的位置，使用保存的位置
            if (savedX >= 0 && savedY >= 0)
            {
                rect = new Rect(savedX, savedY, savedWidth, savedHeight);
            }
            else
            {
                // 否則置中於螢幕
                var mainWindowRect = EditorGUIUtility.GetMainWindowPosition();
                var centerX = mainWindowRect.x + (mainWindowRect.width - savedWidth) / 2;
                var centerY = mainWindowRect.y + (mainWindowRect.height - savedHeight) / 2;
                rect = new Rect(centerX, centerY, savedWidth, savedHeight);
            }

            instance.position = rect;
            instance.previousPosition = rect;

            instance.LoadAssetsFromCache();
        }

        private void OnEnable()
        {
            // 從 EditorPrefs 載入保存的搜尋字串和模式
            searchString = EditorPrefs.GetString(SEARCH_STRING_PREF_KEY, "");
            prevSearchString = "";
            currentMode = (SearchMode)
                EditorPrefs.GetInt(SEARCH_MODE_PREF_KEY, (int)SearchMode.Prefabs);

            // 初始化位置追蹤
            previousPosition = position;

            LoadAssetsFromCache();
        }

        private void Update()
        {
            // 監聽視窗位置和尺寸變化
            if (position != previousPosition)
            {
                SaveWindowState();
                previousPosition = position;
            }
        }

        private void SaveWindowState()
        {
            EditorPrefs.SetFloat(WINDOW_WIDTH_PREF_KEY, position.width);
            EditorPrefs.SetFloat(WINDOW_HEIGHT_PREF_KEY, position.height);
            EditorPrefs.SetFloat(WINDOW_X_PREF_KEY, position.x);
            EditorPrefs.SetFloat(WINDOW_Y_PREF_KEY, position.y);
        }

        private readonly Stopwatch onGuiStopwatch = new();

        private void OnGUI()
        {
            float keyboardInputTime,
                drawModeTabTime,
                drawSearchFieldTime,
                drawRefreshButtonTime,
                drawAssetListTime,
                updateSearchTime,
                handleContextMenuTime;

            onGuiStopwatch.Restart();

            guiStopwatch.Restart();
            HandleKeyboardInput();
            guiStopwatch.Stop();
            keyboardInputTime = guiStopwatch.ElapsedMilliseconds;

            guiStopwatch.Restart();
            DrawModeTab();
            guiStopwatch.Stop();
            drawModeTabTime = guiStopwatch.ElapsedMilliseconds;

            guiStopwatch.Restart();
            DrawSearchField();
            guiStopwatch.Stop();
            drawSearchFieldTime = guiStopwatch.ElapsedMilliseconds;

            guiStopwatch.Restart();
            DrawRefreshButton();
            guiStopwatch.Stop();
            drawRefreshButtonTime = guiStopwatch.ElapsedMilliseconds;

            guiStopwatch.Restart();
            DrawAssetList();
            guiStopwatch.Stop();
            drawAssetListTime = guiStopwatch.ElapsedMilliseconds;

            guiStopwatch.Restart();
            UpdateSearch();
            guiStopwatch.Stop();
            updateSearchTime = guiStopwatch.ElapsedMilliseconds;

            guiStopwatch.Restart();
            HandleContextMenu();
            guiStopwatch.Stop();
            handleContextMenuTime = guiStopwatch.ElapsedMilliseconds;

            onGuiStopwatch.Stop();
            // 每100幀記錄一次GUI重繪時間
            // if (Time.frameCount % 100 == 0)
            if (guiStopwatch.ElapsedMilliseconds > 100)
            {
                Debug.Log(
                    $"[CommandPalette] GUI 重繪時間: {onGuiStopwatch.ElapsedMilliseconds}ms (Frame: {Time.frameCount})"
                );
                Debug.Log($"[CommandPalette] 鍵盤輸入處理時間: {keyboardInputTime}ms");
                Debug.Log($"[CommandPalette] 標籤頁繪製時間: {drawModeTabTime}ms");
                Debug.Log($"[CommandPalette] 搜尋欄繪製時間: {drawSearchFieldTime}ms");
                Debug.Log($"[CommandPalette] 重新整理按鈕繪製時間: {drawRefreshButtonTime}ms");
                Debug.Log($"[CommandPalette] 資源列表繪製時間: {drawAssetListTime}ms");
                Debug.Log($"[CommandPalette] 搜尋更新時間: {updateSearchTime}ms");
                Debug.Log($"[CommandPalette] 上下文菜單處理時間: {handleContextMenuTime}ms");
            }
        }

        private void HandleKeyboardInput()
        {
            if (Event.current.type == EventType.KeyDown)
                switch (Event.current.keyCode)
                {
                    case KeyCode.Escape:
                        Close();
                        Event.current.Use();
                        break;

                    case KeyCode.Tab:
                        // 循環切換搜尋模式
                        var currentIndex = SupportedAssetTypes.FindIndex(x =>
                            x.Mode == currentMode
                        );
                        var nextIndex = (currentIndex + 1) % SupportedAssetTypes.Count;
                        currentMode = SupportedAssetTypes[nextIndex].Mode;

                        // 保存當前搜尋模式到 EditorPrefs
                        EditorPrefs.SetInt(SEARCH_MODE_PREF_KEY, (int)currentMode);

                        LoadAssetsFromCache();
                        selectedIndex = -1;

                        // 重新 focus 到搜尋欄
                        if (searchField != null)
                            searchField.SetFocus();
                        Event.current.Use();
                        prevSearchString = "";
                        UpdateSearch();
                        Repaint();
                        break;

                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        if (selectedIndex >= 0 && selectedIndex < filteredAssets.Count)
                        {
                            OpenAssetInEditor(filteredAssets[selectedIndex]);
                            Event.current.Use();
                        }

                        break;

                    case KeyCode.UpArrow:
                        selectedIndex =
                            selectedIndex <= 0 ? filteredAssets.Count - 1 : selectedIndex - 1;
                        Event.current.Use();
                        EditorGUIUtility.PingObject(filteredAssets[selectedIndex].asset);
                        Repaint();
                        break;

                    case KeyCode.DownArrow:
                        selectedIndex =
                            selectedIndex >= filteredAssets.Count - 1 ? 0 : selectedIndex + 1;
                        Event.current.Use();
                        EditorGUIUtility.PingObject(filteredAssets[selectedIndex].asset);
                        Repaint();
                        break;
                }
        }

        private void DrawModeTab()
        {
            var tabRect = new Rect(0, 0, position.width, TAB_HEIGHT);
            var tabCount = SupportedAssetTypes.Count;
            var tabWidth = position.width / tabCount;

            // 繪製背景
            EditorGUI.DrawRect(tabRect, new Color(0.2f, 0.2f, 0.2f, 1f));

            // 繪製標籤頁
            var originalColor = GUI.color;

            for (int i = 0; i < SupportedAssetTypes.Count; i++)
            {
                var assetType = SupportedAssetTypes[i];
                var tabButtonRect = new Rect(i * tabWidth, 0, tabWidth, TAB_HEIGHT);

                // 設置顏色
                if (currentMode == assetType.Mode)
                    GUI.color = new Color(0.4f, 0.6f, 1f, 1f); // 藍色表示選中
                else
                    GUI.color = new Color(0.7f, 0.7f, 0.7f, 1f);

                if (GUI.Button(tabButtonRect, assetType.DisplayName, EditorStyles.toolbarButton))
                {
                    if (currentMode != assetType.Mode)
                    {
                        currentMode = assetType.Mode;

                        // 保存當前搜尋模式到 EditorPrefs
                        EditorPrefs.SetInt(SEARCH_MODE_PREF_KEY, (int)currentMode);

                        LoadAssetsFromCache();
                        selectedIndex = -1;
                        // 重新 focus 到搜尋欄
                        if (searchField != null)
                            searchField.SetFocus();
                        Repaint();
                    }
                }
            }

            GUI.color = originalColor;
        }

        private void DrawSearchField()
        {
            if (searchField == null)
            {
                searchField = new SearchField();
                searchField.SetFocus();
            }

            var searchRect = new Rect(5, TAB_HEIGHT + 5, position.width - 70, 18); // 預留空間給重新整理按鈕，並考慮標籤頁高度
            searchString = searchField.OnGUI(searchRect, searchString);
            // 保存搜尋字串到 EditorPrefs
            EditorPrefs.SetString(SEARCH_STRING_PREF_KEY, searchString);
        }

        private void DrawAssetList()
        {
            var listStartY = TAB_HEIGHT + 28; // 考慮標籤頁和搜尋欄的高度
            var listRect = new Rect(0, listStartY, position.width, position.height - listStartY);
            var contentRect = new Rect(
                0,
                0,
                position.width - 20,
                filteredAssets.Count * ROW_HEIGHT
            );

            scrollPos = GUI.BeginScrollView(listRect, scrollPos, contentRect);

            for (var i = 0; i < filteredAssets.Count; i++)
            {
                var asset = filteredAssets[i];
                var rect = new Rect(0, i * ROW_HEIGHT, position.width - 20, ROW_HEIGHT);

                // 繪製選中狀態背景
                if (i == selectedIndex)
                    EditorGUI.DrawRect(rect, new Color(0.3f, 0.5f, 0.85f, 0.8f));
                else if (rect.Contains(Event.current.mousePosition))
                    EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.3f));

                // 繪製圖標 - 延遲載入
                var iconRect = new Rect(rect.x + 5, rect.y + 3, 16, 16);
                asset.LoadIconIfNeeded();
                if (asset.icon != null)
                    GUI.DrawTexture(iconRect, asset.icon);

                // 繪製名稱
                var nameRect = new Rect(rect.x + 25, rect.y, rect.width - 30, rect.height);

                // 計算名稱文字寬度
                var nameContent = new GUIContent(asset.name);
                var nameWidth = GUI.skin.label.CalcSize(nameContent).x;

                // 繪製資源名稱
                var actualNameRect = new Rect(nameRect.x, nameRect.y, nameWidth, nameRect.height);
                GUI.Label(actualNameRect, asset.name);

                // 繪製路徑（灰色文字）
                var pathRect = new Rect(
                    actualNameRect.xMax + 10,
                    nameRect.y,
                    nameRect.width - nameWidth - 10,
                    nameRect.height
                );
                if (pathRect.width > 0)
                {
                    var originalColor = GUI.color;
                    GUI.color = new Color(0.6f, 0.6f, 0.6f, 0.6f); // 灰色

                    var pathContent = new GUIContent(asset.path);
                    var pathStyle = new GUIStyle(GUI.skin.label) { fontSize = 10 }; // 較小字體

                    GUI.Label(pathRect, pathContent, pathStyle);
                    GUI.color = originalColor;
                }

                // 處理拖曳操作
                HandleDragAndDrop(rect, asset);

                // 處理滑鼠點擊
                if (
                    Event.current.type == EventType.MouseDown
                    && rect.Contains(Event.current.mousePosition)
                )
                {
                    selectedIndex = i;

                    // Ping 選中的 Object 以在 Project 視窗中高亮顯示
                    EditorGUIUtility.PingObject(asset.asset);

                    if (Event.current.clickCount == 2)
                        OpenAssetInEditor(asset);
                    Event.current.Use();
                    Repaint();
                }
            }

            GUI.EndScrollView();
        }

        /// <summary>
        /// 處理拖曳操作
        /// </summary>
        /// <param name="rect">項目的矩形區域</param>
        /// <param name="asset">要拖曳的資源</param>
        private void HandleDragAndDrop(Rect rect, AssetEntry asset)
        {
            var controlID = GUIUtility.GetControlID(FocusType.Passive, rect);
            var eventType = Event.current.GetTypeForControl(controlID);

            switch (eventType)
            {
                case EventType.MouseDown:
                    if (rect.Contains(Event.current.mousePosition) && Event.current.button == 0)
                    {
                        // 開始拖曳準備
                        GUIUtility.hotControl = controlID;
                        DragAndDrop.PrepareStartDrag();
                    }
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID && Event.current.button == 0)
                    {
                        // 設置拖曳資料
                        DragAndDrop.objectReferences = new Object[] { asset.asset };
                        DragAndDrop.paths = new string[] { asset.path };

                        // 設置拖曳的視覺標題
                        DragAndDrop.SetGenericData("AssetEntry", asset);
                        DragAndDrop.StartDrag(asset.name);

                        // 釋放控制
                        GUIUtility.hotControl = 0;
                        Event.current.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID)
                    {
                        GUIUtility.hotControl = 0;
                    }
                    break;
            }
        }

        private void LoadAssetsFromCache()
        {
            if (currentMode == SearchMode.MenuItems)
            {
                LoadMenuItemsFromCache();
                return;
            }

            var assetTypeDefinition = GetAssetTypeDefinition(currentMode);
            var currentCache = AssetCaches[currentMode];
            var isCacheValid = CacheValidFlags[currentMode];
            var hasPendingChanges = PendingChangesFlags[currentMode];

            // 檢查記憶體快取是否有效
            if (isCacheValid)
            {
                Debug.Log(
                    $"[CommandPalette] {assetTypeDefinition.DisplayName} 快取有效，直接使用記憶體快取"
                );
                // 如果有待處理的變更，表示 AssetPostprocessor 已經通過 AddOrUpdateAssetInCache/RemoveAssetFromCache 更新了快取並寫入檔案
                if (hasPendingChanges)
                {
                    Debug.Log(
                        $"[CommandPalette] 檢測到待處理的 {assetTypeDefinition.DisplayName} 變更，快取已通過增量更新保持最新狀態，檔案快取也已同步"
                    );

                    // 快取已同步完成，清除待處理標誌
                    PendingChangesFlags[currentMode] = false;
                }
                else
                {
                    Debug.Log(
                        $"[CommandPalette] {assetTypeDefinition.DisplayName} 快取無待處理變更"
                    );
                }

                allAssets = new List<AssetEntry>(currentCache.Values);
                filteredAssets = new List<AssetEntry>(allAssets);
                return;
            }

            Debug.Log(
                $"[CommandPalette] {assetTypeDefinition.DisplayName} 快取無效或過期，重新掃描資源"
            );

            // 嘗試從檔案快取載入
            cacheLoadStopwatch.Restart();
            var cacheFilePath = GetCacheFilePath(currentMode);
            var fileCache = SearchCommandPaletteCacheHelper.LoadCacheFromFile(
                cacheFilePath,
                currentMode
            );
            if (fileCache != null)
            {
                var dictConversionStart = cacheLoadStopwatch.ElapsedMilliseconds;
                AssetCaches[currentMode] = fileCache.ToDictionary(asset => asset.guid);
                var dictConversionTime =
                    cacheLoadStopwatch.ElapsedMilliseconds - dictConversionStart;

                CacheValidFlags[currentMode] = true;
                PendingChangesFlags[currentMode] = false; // 從檔案載入後清除待處理標誌

                var listConversionStart = cacheLoadStopwatch.ElapsedMilliseconds;
                allAssets = new List<AssetEntry>(AssetCaches[currentMode].Values);
                filteredAssets = new List<AssetEntry>(allAssets);
                var listConversionTime =
                    cacheLoadStopwatch.ElapsedMilliseconds - listConversionStart;

                cacheLoadStopwatch.Stop();
                Debug.Log(
                    $"[CommandPalette] {assetTypeDefinition.DisplayName} 快取載入完成: 總時間 {cacheLoadStopwatch.ElapsedMilliseconds}ms, "
                        + $"Dictionary轉換 {dictConversionTime}ms, List轉換 {listConversionTime}ms, 資源數量 {fileCache.Count}"
                );
                return;
            }

            Debug.LogError(
                $"[CommandPalette] 無法從檔案快取載入 {assetTypeDefinition.DisplayName}，將重新收集資源"
            );

            // 快取無效或過期，重新掃描資源
            PendingChangesFlags[currentMode] = false; // 重新掃描後清除待處理標誌
            RefreshAssetsInternal();
        }

        private void LoadMenuItemsFromCache()
        {
            // 檢查記憶體快取是否有效
            if (menuItemCacheValid && menuItemCache.Count > 0)
            {
                ConvertMenuItemsToAssets();
                return;
            }

            // 嘗試從檔案快取載入MenuItem
            cacheLoadStopwatch.Restart();
            var cacheFilePath = GetCacheFilePath(SearchMode.MenuItems);
            var fileCache = SearchCommandPaletteCacheHelper.LoadMenuItemCacheFromFile(
                cacheFilePath
            );
            if (fileCache != null && fileCache.Count > 0)
            {
                menuItemCache = fileCache;
                menuItemCacheValid = true;
                ConvertMenuItemsToAssets();

                cacheLoadStopwatch.Stop();
                Debug.Log(
                    $"[CommandPalette] MenuItem 快取載入完成，共 {fileCache.Count} 個項目，耗時 {cacheLoadStopwatch.ElapsedMilliseconds}ms"
                );
                return;
            }

            // 快取無效或過期，重新收集MenuItem
            RefreshMenuItems();
        }

        private void ConvertMenuItemsToAssets()
        {
            allAssets = menuItemCache
                .Select(menuItem => new AssetEntry(menuItem.displayName, menuItem.menuPath, null))
                .ToList();
            filteredAssets = new List<AssetEntry>(allAssets);
        }

        private void RefreshMenuItems()
        {
            cacheLoadStopwatch.Restart();
            menuItemCache = SearchCommandPaletteCacheHelper.CollectAllMenuItems();
            menuItemCacheValid = true;

            ConvertMenuItemsToAssets();

            // 儲存到檔案快取
            var cacheFilePath = GetCacheFilePath(SearchMode.MenuItems);
            SearchCommandPaletteCacheHelper.SaveMenuItemCacheToFile(menuItemCache, cacheFilePath);

            cacheLoadStopwatch.Stop();
            Debug.Log(
                $"[CommandPalette] MenuItem 重新收集完成，共 {menuItemCache.Count} 個項目，耗時 {cacheLoadStopwatch.ElapsedMilliseconds}ms"
            );
        }

        private void UpdateSearch()
        {
            if (searchString == prevSearchString)
                return;

            prevSearchString = searchString;

            if (string.IsNullOrEmpty(searchString))
            {
                filteredAssets = new List<AssetEntry>(allAssets);
            }
            else
            {
                var searchResults = new List<(AssetEntry asset, long score)>();

                // 預處理搜尋字串：支援多詞搜尋，將空格替換為下劃線進行匹配
                var normalizedSearch = searchString.ToLowerInvariant();
                var spaceToUnderscoreSearch = normalizedSearch.Replace(' ', '_');

                foreach (var asset in allAssets)
                {
                    var assetNameLower = asset.name.ToLowerInvariant();

                    // 使用 Unity 的 FuzzySearch API
                    var normalMatch = FuzzySearch.FuzzyMatch(normalizedSearch, assetNameLower);
                    var spaceMatch = FuzzySearch.FuzzyMatch(
                        spaceToUnderscoreSearch,
                        assetNameLower
                    );

                    // 如果任一匹配成功，計算優先級分數
                    if (normalMatch || spaceMatch)
                    {
                        // 使用匹配優先級作為分數（數字越小優先級越高，所以用負數讓 ThenByDescending 正確排序）
                        var priority = GetMatchPriority(asset.name, searchString);
                        var spaceToUnderscorePriority = GetMatchPriority(
                            asset.name,
                            spaceToUnderscoreSearch
                        );
                        var bestPriority = Math.Min(priority, spaceToUnderscorePriority);

                        searchResults.Add((asset, -bestPriority)); // 負數讓優先級高的排在前面
                    }
                }

                // 智慧排序：優先級越高（數字越小）排在前面，然後按名稱排序
                filteredAssets = searchResults
                    .OrderByDescending(x => x.score) // score 已經是負的優先級，所以用 Descending
                    .ThenBy(x => x.asset.name)
                    .Select(x => x.asset)
                    .ToList();
            }

            selectedIndex = filteredAssets.Count > 0 ? 0 : -1;
            Repaint();
        }

        // private bool TryFuzzyMatch(string targetName, string query, int[] matchIndexes, ref float cost)
        // {
        //     var wordInitialsIndexes = new List<int> { 0 };
        //
        //     for (var i = 1; i < targetName.Length; i++)
        //     {
        //         var separators = new[] { ' ', '-', '_', '.', '(', ')', '[', ']' };
        //
        //         var prevChar = targetName[i - 1];
        //         var curChar = targetName[i];
        //         var nextChar = i + 1 < targetName.Length ? targetName[i + 1] : default;
        //
        //         var isSeparatedWordStart = separators.Contains(prevChar) && !separators.Contains(curChar);
        //         var isCamelcaseHump = (char.IsUpper(curChar) && char.IsLower(prevChar)) ||
        //                               (char.IsUpper(curChar) && char.IsLower(nextChar));
        //         var isNumberStart = char.IsDigit(curChar) && (!char.IsDigit(prevChar) || prevChar == '0');
        //         var isAfterNumber = char.IsDigit(prevChar) && !char.IsDigit(curChar);
        //
        //         if (isSeparatedWordStart || isCamelcaseHump || isNumberStart || isAfterNumber)
        //             wordInitialsIndexes.Add(i);
        //     }
        //
        //     var nextWordInitialsIndexMap = new int[targetName.Length];
        //     var nextWordIndex = 0;
        //
        //     for (var i = 0; i < targetName.Length; i++)
        //     {
        //         if (i == wordInitialsIndexes[nextWordIndex])
        //             if (nextWordIndex + 1 < wordInitialsIndexes.Count)
        //                 nextWordIndex++;
        //             else break;
        //
        //         nextWordInitialsIndexMap[i] = wordInitialsIndexes[nextWordIndex];
        //     }
        //
        //     var iName = 0;
        //     var iQuery = 0;
        //     var prevMatchIndex = -1;
        //
        //     cost = 0;
        //
        //     while (iName < targetName.Length && iQuery < query.Length)
        //     {
        //         var curQuerySymbol = char.ToLower(query[iQuery]);
        //         var curNameSymbol = char.ToLower(targetName[iName]);
        //
        //         if (curNameSymbol == curQuerySymbol)
        //         {
        //             var gapLength = iName - prevMatchIndex - 1;
        //             cost += gapLength;
        //
        //             matchIndexes[iQuery] = iName;
        //             iQuery++;
        //             iName = iName + 1;
        //             prevMatchIndex = iName - 1;
        //             continue;
        //         }
        //
        //         var nextWordInitialIndex = nextWordInitialsIndexMap[iName];
        //         var nextWordInitialSymbol = nextWordInitialIndex == default
        //             ? default
        //             : char.ToLower(targetName[nextWordInitialIndex]);
        //
        //         if (nextWordInitialSymbol == curQuerySymbol)
        //         {
        //             var gapLength = nextWordInitialIndex - prevMatchIndex - 1;
        //             cost += gapLength * 0.01f;
        //
        //             matchIndexes[iQuery] = nextWordInitialIndex;
        //             iQuery++;
        //             iName = nextWordInitialIndex + 1;
        //             prevMatchIndex = nextWordInitialIndex;
        //             continue;
        //         }
        //
        //         iName++;
        //     }
        //
        //     return iQuery >= query.Length;
        // }

        private void RefreshAssetsInternal()
        {
            // MenuItem 模式使用專用的刷新方法
            if (currentMode == SearchMode.MenuItems)
            {
                RefreshMenuItems();
                return;
            }

            isRefreshing = true;
            var tempAssets = new List<AssetEntry>();
            var stopwatch = Stopwatch.StartNew();
            var assetTypeDefinition = GetAssetTypeDefinition(currentMode);

            Debug.Log($"[CommandPalette] 開始掃描 {assetTypeDefinition.DisplayName} 資源...");

            try
            {
                // 階段1：統一搜尋所有資源（Assets + Packages）
                var phase1Start = stopwatch.ElapsedMilliseconds;
                var allAssetGuids = AssetDatabase.FindAssets(
                    assetTypeDefinition.AssetDatabaseFilter
                );
                Debug.Log(
                    $"[CommandPalette] 搜尋完成，找到 {allAssetGuids.Length} 個 {assetTypeDefinition.DisplayName}，耗時 {stopwatch.ElapsedMilliseconds - phase1Start}ms"
                );

                // 階段2：載入所有資源
                var phase2Start = stopwatch.ElapsedMilliseconds;
                var assetsCount = 0;
                var packagesCount = 0;

                foreach (var guid in allAssetGuids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath(path, assetTypeDefinition.UnityType);
                    if (asset != null)
                    {
                        tempAssets.Add(new AssetEntry(asset.name, path, asset));
                        if (path.StartsWith("Packages/"))
                            packagesCount++;
                        else
                            assetsCount++;
                    }
                }
                Debug.Log(
                    $"[CommandPalette] 載入 {assetTypeDefinition.DisplayName} 完成，Assets: {assetsCount} 個，Packages: {packagesCount} 個，總計 {tempAssets.Count} 個，耗時 {stopwatch.ElapsedMilliseconds - phase2Start}ms"
                );

                // 階段3：智慧排序（Assets 資料夾優先，然後按名稱）
                var sortStart = stopwatch.ElapsedMilliseconds;
                tempAssets = tempAssets
                    .OrderBy(p => p.path.StartsWith("Packages/") ? 1 : 0) // Assets 資料夾優先於 Packages
                    .ThenBy(p => p.name)
                    .ToList();
                Debug.Log(
                    $"[CommandPalette] 排序完成，耗時 {stopwatch.ElapsedMilliseconds - sortStart}ms"
                );

                // 更新快取
                AssetCaches[currentMode] = tempAssets.ToDictionary(asset => asset.guid);
                CacheValidFlags[currentMode] = true;
                PendingChangesFlags[currentMode] = false;

                // 儲存檔案快取
                var cacheFilePath = GetCacheFilePath(currentMode);
                SearchCommandPaletteCacheHelper.SaveCacheToFile(tempAssets, cacheFilePath);

                // 更新實例資料
                allAssets = new List<AssetEntry>(tempAssets);
                filteredAssets = new List<AssetEntry>(allAssets);

                stopwatch.Stop();
                Debug.Log(
                    $"[CommandPalette] {assetTypeDefinition.DisplayName} 資源掃描完成，總計 {tempAssets.Count} 個資源，總耗時 {stopwatch.ElapsedMilliseconds}ms (不包含縮圖載入)"
                );
            }
            finally
            {
                isRefreshing = false;
            }
        }

        private void DrawRefreshButton()
        {
            var buttonRect = new Rect(position.width - 60, TAB_HEIGHT + 5, 55, 18);

            if (isRefreshing)
            {
                GUI.enabled = false;
                GUI.Button(buttonRect, "載入中...");
                GUI.enabled = true;
            }
            else if (GUI.Button(buttonRect, "重新整理"))
            {
                RefreshAssetsInternal();
                Repaint();
            }
        }

        private void HandleContextMenu()
        {
            if (Event.current.type == EventType.ContextClick)
            {
                var menu = new GenericMenu();
                var assetTypeDefinition = GetAssetTypeDefinition(currentMode);

                menu.AddItem(
                    new GUIContent($"重新整理 {assetTypeDefinition.DisplayName} 清單"),
                    false,
                    () =>
                    {
                        RefreshAssetsInternal();
                        Repaint();
                    }
                );

                menu.AddItem(
                    new GUIContent("清除快取"),
                    false,
                    () =>
                    {
                        CacheValidFlags[currentMode] = false;
                        AssetCaches[currentMode].Clear();
                        PendingChangesFlags[currentMode] = false;
                        RefreshAssetsInternal();
                        Repaint();
                    }
                );

                menu.ShowAsContext();
                Event.current.Use();
            }
        }

        private void OpenAssetInEditor(AssetEntry assetEntry)
        {
            try
            {
                // MenuItem 模式：執行選定的 MenuItem
                if (currentMode == SearchMode.MenuItems)
                {
                    var menuPath = assetEntry.path; // 我們使用guid存儲menuPath
                    if (!string.IsNullOrEmpty(menuPath))
                    {
                        // 找到對應的MenuItem並執行
                        var menuItem = menuItemCache.FirstOrDefault(m => m.menuPath == menuPath);
                        if (menuItem != null && menuItem.isValidated && menuItem.isEnabled)
                        {
                            EditorApplication.ExecuteMenuItem(menuPath);

                            Debug.Log($"🎨已執行MenuItem: {menuItem.displayName} -> {menuPath}");
                        }
                        else
                        {
                            Debug.LogWarning($"MenuItem無法執行: {menuPath}");
                        }
                    }
                    Close();
                    return;
                }

                if (assetEntry.assetType == typeof(GameObject) && currentMode == SearchMode.Prefabs)
                {
                    // 對於 Prefab，嘗試使用 PrefabStageUtility 進入 Prefab 模式 (Unity 2018.3+)
                    var sceneManagementAssembly = typeof(EditorSceneManager).Assembly;
                    var prefabStageUtilityType = sceneManagementAssembly.GetType(
                        "UnityEditor.SceneManagement.PrefabStageUtility"
                    );

                    if (prefabStageUtilityType != null)
                    {
                        var openPrefabMethod = prefabStageUtilityType.GetMethod(
                            "OpenPrefab",
                            BindingFlags.Public | BindingFlags.Static,
                            null,
                            new[] { typeof(string) },
                            null
                        );

                        if (openPrefabMethod != null)
                        {
                            var result = openPrefabMethod.Invoke(
                                null,
                                new object[] { assetEntry.path }
                            );
                            if (result != null)
                            {
                                Debug.Log($"已在 Prefab 模式中打開: {assetEntry.name}");
                                Close();
                                return;
                            }
                        }
                    }

                    // 如果 PrefabStageUtility 不可用，嘗試使用傳統方法
                    Debug.LogWarning(
                        "PrefabStageUtility 不可用，使用 AssetDatabase.OpenAsset 作為回退方案"
                    );
                }

                // 對於 ScriptableObject 或其他資源，直接使用 AssetDatabase.OpenAsset
                AssetDatabase.OpenAsset(assetEntry.asset);
                Debug.Log($"已打開資源: {assetEntry.name} ({assetEntry.assetType.Name})");
            }
            catch (Exception e)
            {
                Debug.LogError($"無法打開資源 {assetEntry.name}: {e.Message}");
                // 使用回退方案
                if (currentMode != SearchMode.MenuItems)
                {
                    AssetDatabase.OpenAsset(assetEntry.asset);
                }
            }

            Close();
        }

        /// <summary>
        /// 取得匹配優先級（數字越小優先級越高）
        /// 0 = 完全匹配
        /// 1 = 大小寫無關完全匹配
        /// 2 = 開頭匹配
        /// 3 = 大小寫無關開頭匹配
        /// 4 = 包含匹配
        /// 5 = 大小寫無關包含匹配
        /// 6 = 模糊匹配
        /// </summary>
        private int GetMatchPriority(string assetName, string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return 6;

            var assetNameLower = assetName.ToLowerInvariant();
            var searchTermLower = searchTerm.ToLowerInvariant();

            // 支援空格和下劃線互換的搜尋
            var normalizedAssetName = assetNameLower.Replace('_', ' ');
            var normalizedSearchTerm = searchTermLower.Replace('_', ' ');

            // 完全匹配
            if (assetNameLower == searchTermLower || normalizedAssetName == normalizedSearchTerm)
                return 0;

            // 開頭匹配
            if (
                assetNameLower.StartsWith(searchTermLower)
                || normalizedAssetName.StartsWith(normalizedSearchTerm)
            )
                return 2;

            // 包含匹配
            if (
                assetNameLower.Contains(searchTermLower)
                || normalizedAssetName.Contains(normalizedSearchTerm)
            )
                return 4;

            // 模糊匹配（已通過 FuzzyMatch 驗證）
            return 6;
        }

        private void OnLostFocus()
        {
            Close();
        }

        private void OnDestroy()
        {
            // 保存視窗狀態
            SaveWindowState();

            // 清除靜態引用
            if (instance == this)
                instance = null;
        }

        [InitializeOnLoadMethod]
        private static void RegisterAssetChangeListener()
        {
            if (!isAssetChangeListenerRegistered)
            {
                isAssetChangeListenerRegistered = true;
                // 註冊 AssetDatabase 變更事件
                AssetDatabase.importPackageCompleted += OnPackageImported;
                AssetDatabase.importPackageCancelled += OnPackageImportCancelled;
                AssetDatabase.importPackageFailed += OnPackageImportFailed;

                // 註冊資源修改監聽
                EditorApplication.projectChanged += OnProjectChanged;

                Debug.Log("[CommandPalette] AssetDatabase 事件監聽器已註冊");
            }
        }

        private static void OnPackageImported(string packageName)
        {
            InvalidateAllCaches();
        }

        private static void OnPackageImportCancelled(string packageName)
        {
            // 可能需要重新檢查快取
        }

        private static void OnPackageImportFailed(string packageName, string errorMessage)
        {
            // 導入失敗，可能需要清理快取
        }

        private static void OnProjectChanged()
        {
            // 專案有變更，保守起見使快取失效
            // InvalidateAllCaches();
        }

        /// <summary>
        ///     動態添加或更新單個資源到快取中
        /// </summary>
        /// <param name="assetPath">資源路徑</param>
        public static void AddOrUpdateAssetInCache(string assetPath)
        {
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
                return;

            // 找到對應的資源類型定義
            AssetTypeDefinition matchingType = null;
            foreach (var assetType in SupportedAssetTypes)
            {
                if (assetPath.EndsWith(assetType.FileExtension))
                {
                    matchingType = assetType;
                    break;
                }
            }

            if (matchingType == null)
                return;

            // 載入資源
            var asset = AssetDatabase.LoadAssetAtPath(assetPath, matchingType.UnityType);
            if (asset != null)
            {
                var entry = new AssetEntry(asset.name, assetPath, asset);

                // 確保記憶體快取已載入，避免覆蓋現有快取
                if (!CacheValidFlags[matchingType.Mode])
                {
                    Debug.Log(
                        $"[CommandPalette] {matchingType.DisplayName} 記憶體快取未載入，先從檔案載入避免覆蓋"
                    );
                    var cacheFilePath = GetCacheFilePath(matchingType.Mode);
                    var existingCache = SearchCommandPaletteCacheHelper.LoadCacheFromFile(
                        cacheFilePath,
                        matchingType.Mode
                    );
                    if (existingCache != null && existingCache.Count > 0)
                    {
                        AssetCaches[matchingType.Mode] = existingCache.ToDictionary(cacheEntry =>
                            cacheEntry.guid
                        );
                        CacheValidFlags[matchingType.Mode] = true;
                        Debug.Log(
                            $"[CommandPalette] 已從檔案載入 {existingCache.Count} 個 {matchingType.DisplayName} 快取項目"
                        );
                    }
                }

                AssetCaches[matchingType.Mode][guid] = entry;
                PendingChangesFlags[matchingType.Mode] = true;

                // 立即寫入檔案快取以防 domain reload 導致記憶體快取丟失
                var writeStopwatch = Stopwatch.StartNew();
                try
                {
                    var cacheFilePath = GetCacheFilePath(matchingType.Mode);
                    var cacheList = new List<AssetEntry>(AssetCaches[matchingType.Mode].Values);
                    SearchCommandPaletteCacheHelper.SaveCacheToFile(cacheList, cacheFilePath);
                    writeStopwatch.Stop();

                    Debug.Log(
                        $"[CommandPalette] 已添加/更新 {matchingType.DisplayName} 快取: {assetPath} (總快取: {cacheList.Count} 項，寫入耗時: {writeStopwatch.ElapsedMilliseconds}ms)"
                    );
                }
                catch (Exception ex)
                {
                    writeStopwatch.Stop();
                    Debug.LogError(
                        $"[CommandPalette] 寫入 {matchingType.DisplayName} 檔案快取失敗: {ex.Message} (嘗試耗時: {writeStopwatch.ElapsedMilliseconds}ms)"
                    );
                }

                // 只有在視窗開啟且當前模式匹配時才立即更新UI
                if (instance != null && instance.currentMode == matchingType.Mode)
                {
                    instance.allAssets = new List<AssetEntry>(
                        AssetCaches[matchingType.Mode].Values
                    );
                    instance.UpdateSearch();
                    instance.Repaint();
                }
            }
        }

        /// <summary>
        ///     從快取中移除資源
        /// </summary>
        /// <param name="assetPath">資源路徑</param>
        public static void RemoveAssetFromCache(string assetPath)
        {
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
                return;

            var removed = false;

            // 找到對應的資源類型定義
            foreach (var assetType in SupportedAssetTypes)
            {
                if (assetPath.EndsWith(assetType.FileExtension))
                {
                    // 確保記憶體快取已載入，避免遺漏要移除的項目
                    if (!CacheValidFlags[assetType.Mode])
                    {
                        Debug.Log(
                            $"[CommandPalette] {assetType.DisplayName} 記憶體快取未載入，先從檔案載入避免遺漏移除項目"
                        );
                        var cacheFilePath = GetCacheFilePath(assetType.Mode);
                        var existingCache = SearchCommandPaletteCacheHelper.LoadCacheFromFile(
                            cacheFilePath,
                            assetType.Mode
                        );
                        if (existingCache != null && existingCache.Count > 0)
                        {
                            AssetCaches[assetType.Mode] = existingCache.ToDictionary(cacheEntry =>
                                cacheEntry.guid
                            );
                            CacheValidFlags[assetType.Mode] = true;
                            Debug.Log(
                                $"[CommandPalette] 已從檔案載入 {existingCache.Count} 個 {assetType.DisplayName} 快取項目"
                            );
                        }
                    }

                    if (AssetCaches[assetType.Mode].ContainsKey(guid))
                    {
                        AssetCaches[assetType.Mode].Remove(guid);
                        removed = true;
                        PendingChangesFlags[assetType.Mode] = true;

                        // 立即寫入檔案快取以防 domain reload 導致記憶體快取丟失
                        var writeStopwatch = Stopwatch.StartNew();
                        try
                        {
                            var cacheFilePath = GetCacheFilePath(assetType.Mode);
                            var cacheList = new List<AssetEntry>(
                                AssetCaches[assetType.Mode].Values
                            );
                            SearchCommandPaletteCacheHelper.SaveCacheToFile(
                                cacheList,
                                cacheFilePath
                            );
                            writeStopwatch.Stop();

                            Debug.Log(
                                $"[CommandPalette] 已從 {assetType.DisplayName} 快取移除: {assetPath} (總快取: {cacheList.Count} 項，寫入耗時: {writeStopwatch.ElapsedMilliseconds}ms)"
                            );
                        }
                        catch (Exception ex)
                        {
                            writeStopwatch.Stop();
                            Debug.LogError(
                                $"[CommandPalette] 移除後寫入 {assetType.DisplayName} 檔案快取失敗: {ex.Message} (嘗試耗時: {writeStopwatch.ElapsedMilliseconds}ms)"
                            );
                        }

                        // 只有在視窗開啟且當前模式匹配時才立即更新UI
                        if (instance != null && instance.currentMode == assetType.Mode)
                        {
                            instance.allAssets = new List<AssetEntry>(
                                AssetCaches[assetType.Mode].Values
                            );
                            instance.UpdateSearch();
                            instance.Repaint();
                        }
                        break;
                    }
                }
            }

            if (!removed)
                Debug.Log($"[CommandPalette] 嘗試移除資源但未在快取中找到: {assetPath}");
        }

        private static void InvalidateAllCaches()
        {
            foreach (var mode in CacheValidFlags.Keys.ToList())
            {
                CacheValidFlags[mode] = false;
                PendingChangesFlags[mode] = false;
            }
            Debug.Log("[CommandPalette] 檢測到專案變更，所有快取已失效");
        }

        private static void InvalidateCacheForAsset(string assetPath)
        {
            foreach (var assetType in SupportedAssetTypes)
            {
                if (assetPath.EndsWith(assetType.FileExtension))
                {
                    CacheValidFlags[assetType.Mode] = false;
                    Debug.Log($"[CommandPalette] {assetType.DisplayName} 快取已失效: {assetPath}");
                    break;
                }
            }
        }
    }
}
#endif

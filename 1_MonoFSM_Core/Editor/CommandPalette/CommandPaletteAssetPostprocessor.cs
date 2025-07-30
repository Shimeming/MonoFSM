#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using _1_MonoFSM_Core.Runtime._3_FlagData;
using MonoDebugSetting;
using UnityEditor;
using UnityEngine;

namespace CommandPalette
{
    /// <summary>
    ///     專門監聽資源變更的 AssetPostprocessor
    /// </summary>
    public class CommandPaletteAssetPostprocessor : AssetPostprocessor
    {
        //統一進入點？
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            var assetChangeCounts = new Dictionary<string, int>();

            // 初始化計數器
            assetChangeCounts[".prefab"] = 0;
            assetChangeCounts[".asset"] = 0;
            assetChangeCounts[".unity"] = 0;

            // 處理導入的資源 - 添加到快取
            if (importedAssets != null)
                foreach (var assetPath in importedAssets)
                {
                    if (assetPath.EndsWith(".prefab"))
                    {
                        SearchPrefabCommandPaletteWindow.AddOrUpdateAssetInCache(assetPath);
                        assetChangeCounts[".prefab"]++;
                    }
                    else if (assetPath.EndsWith(".asset"))
                    {
                        var assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                        //如果assetType是ScriptableObject或其子類型
                        if (assetType.IsSubclassOf(typeof(GameFlagBase)))
                        {
                            AllFlagCollection.Instance.AddFlag(
                                AssetDatabase.LoadAssetAtPath<GameFlagBase>(assetPath));
                            Debug.Log($"[CommandPalette] 添加 GameFlagBase: {assetPath}");
                        }

                        SearchPrefabCommandPaletteWindow.AddOrUpdateAssetInCache(assetPath);
                        assetChangeCounts[".asset"]++;
                    }
                    else if (assetPath.EndsWith(".unity"))
                    {
                        SearchPrefabCommandPaletteWindow.AddOrUpdateAssetInCache(assetPath);
                        assetChangeCounts[".unity"]++;
                    }
                }

            // 處理刪除的資源 - 從快取移除
            if (deletedAssets != null)
                foreach (var assetPath in deletedAssets)
                {
                    if (assetPath.EndsWith(".prefab"))
                    {
                        SearchPrefabCommandPaletteWindow.RemoveAssetFromCache(assetPath);
                        assetChangeCounts[".prefab"]++;
                    }
                    else if (assetPath.EndsWith(".asset"))
                    {
                        SearchPrefabCommandPaletteWindow.RemoveAssetFromCache(assetPath);
                        assetChangeCounts[".asset"]++;
                    }
                    else if (assetPath.EndsWith(".unity"))
                    {
                        SearchPrefabCommandPaletteWindow.RemoveAssetFromCache(assetPath);
                        assetChangeCounts[".unity"]++;
                    }
                }

            // 處理移動的資源 - 更新快取中的路徑
            if (movedAssets != null)
                for (var i = 0; i < movedAssets.Length; i++)
                {
                    var newPath = movedAssets[i];
                    var oldPath = i < movedFromAssetPaths.Length ? movedFromAssetPaths[i] : "";

                    if (newPath.EndsWith(".prefab"))
                    {
                        // 先移除舊路徑，再添加新路徑
                        if (!string.IsNullOrEmpty(oldPath))
                            SearchPrefabCommandPaletteWindow.RemoveAssetFromCache(oldPath);
                        SearchPrefabCommandPaletteWindow.AddOrUpdateAssetInCache(newPath);
                        assetChangeCounts[".prefab"]++;
                    }
                    else if (newPath.EndsWith(".asset"))
                    {
                        // 先移除舊路徑，再添加新路徑
                        if (!string.IsNullOrEmpty(oldPath))
                            SearchPrefabCommandPaletteWindow.RemoveAssetFromCache(oldPath);
                        SearchPrefabCommandPaletteWindow.AddOrUpdateAssetInCache(newPath);
                        assetChangeCounts[".asset"]++;
                    }
                    else if (newPath.EndsWith(".unity"))
                    {
                        // 先移除舊路徑，再添加新路徑
                        if (!string.IsNullOrEmpty(oldPath))
                            SearchPrefabCommandPaletteWindow.RemoveAssetFromCache(oldPath);
                        SearchPrefabCommandPaletteWindow.AddOrUpdateAssetInCache(newPath);
                        assetChangeCounts[".unity"]++;
                    }
                }

            // 詳細分析變更（僅在偵錯時）
            if (DebugSetting.IsDebugMode)
            {
                if (importedAssets?.Length > 0)
                    AnalyzeAssetChanges(importedAssets, "導入");
                if (deletedAssets?.Length > 0)
                    AnalyzeAssetChanges(deletedAssets, "刪除");
                if (movedAssets?.Length > 0)
                    AnalyzeAssetChanges(movedAssets, "移動");
            }

            // 記錄變更統計
            if (assetChangeCounts[".prefab"] > 0) 
                Debug.Log($"[CommandPalette] 動態更新 {assetChangeCounts[".prefab"]} 個 Prefab 快取");
            if (assetChangeCounts[".asset"] > 0)
                Debug.Log($"[CommandPalette] 動態更新 {assetChangeCounts[".asset"]} 個 ScriptableObject 快取");
            if (assetChangeCounts[".unity"] > 0)
                Debug.Log($"[CommandPalette] 動態更新 {assetChangeCounts[".unity"]} 個 Scene 快取");
        }

        private static int CountRelevantAssets(string[] paths, string extension)
        {
            if (paths == null) return 0;
            return paths.Count(path => path.EndsWith(extension));
        }

        /// <summary>
        ///     詳細的資源變更分析
        /// </summary>
        internal static void AnalyzeAssetChanges(string[] paths, string changeType)
        {
            if (paths == null || paths.Length == 0) return;

            var prefabPaths = new List<string>();
            var scriptableObjectPaths = new List<string>();
            var scenePaths = new List<string>();
            var otherAssetPaths = new List<string>();

            foreach (var path in paths)
                if (path.EndsWith(".prefab"))
                {
                    prefabPaths.Add(path);
                }
                else if (path.EndsWith(".asset"))
                {
                    // 嘗試判斷 ScriptableObject 類型
                    var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                    if (asset != null)
                        scriptableObjectPaths.Add($"{path} ({asset.GetType().Name})");
                    else
                        scriptableObjectPaths.Add(path);
                }
                else if (path.EndsWith(".unity"))
                {
                    scenePaths.Add(path);
                }
                else if (path.EndsWith(".cs") && path.Contains("ScriptableObject"))
                {
                    // ScriptableObject 腳本變更可能影響現有資源
                    otherAssetPaths.Add(path);
                }

            if (prefabPaths.Count > 0)
                Debug.Log($"[CommandPalette] {changeType} Prefabs ({prefabPaths.Count}):\n  - " +
                          string.Join("\n  - ", prefabPaths.Take(5)) +
                          (prefabPaths.Count > 5 ? $"\n  ... 還有 {prefabPaths.Count - 5} 個" : ""));

            if (scriptableObjectPaths.Count > 0)
                Debug.Log($"[CommandPalette] {changeType} ScriptableObjects ({scriptableObjectPaths.Count}):\n  - " +
                          string.Join("\n  - ", scriptableObjectPaths.Take(5)) + (scriptableObjectPaths.Count > 5
                              ? $"\n  ... 還有 {scriptableObjectPaths.Count - 5} 個"
                              : ""));

            if (scenePaths.Count > 0)
                Debug.Log($"[CommandPalette] {changeType} Scenes ({scenePaths.Count}):\n  - " +
                          string.Join("\n  - ", scenePaths.Take(5)) +
                          (scenePaths.Count > 5 ? $"\n  ... 還有 {scenePaths.Count - 5} 個" : ""));

            if (otherAssetPaths.Count > 0)
                Debug.Log($"[CommandPalette] {changeType} 相關腳本 ({otherAssetPaths.Count}): " +
                          string.Join(", ", otherAssetPaths));
        }
    }
}
#endif
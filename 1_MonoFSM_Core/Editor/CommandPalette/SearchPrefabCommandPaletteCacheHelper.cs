#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace CommandPalette
{
    /// <summary>
    ///     搜尋命令面板快取輔助工具
    /// </summary>
    public static class SearchPrefabCommandPaletteCacheHelper
    {
        // 檔案快取路徑
        private static readonly string CacheDirectory = Path.Combine("Library", "CommandPalette");

        public static void SaveCacheToFile(List<AssetEntry> assets, string cacheFilePath)
        {
            try
            {
                if (!Directory.Exists(CacheDirectory))
                    Directory.CreateDirectory(CacheDirectory);

                var cacheData = new CacheContainer
                {
                    cacheTimestamp = DateTime.Now.Ticks,
                    assets = assets.Select(asset =>
                    {
                        var fileInfo = new FileInfo(asset.path);
                        return new AssetCacheData(
                            asset.name,
                            asset.path,
                            asset.guid,
                            asset.assetType.AssemblyQualifiedName,
                            fileInfo.Exists ? fileInfo.LastWriteTime.Ticks : 0);
                    }).ToList()
                };

                var json = JsonUtility.ToJson(cacheData, true);
                File.WriteAllText(cacheFilePath, json);
                Debug.Log($"[CommandPalette] 快取已儲存至 {cacheFilePath}，包含 {assets.Count} 個資源");
            }
            catch (Exception e)
            {
                Debug.LogError($"[CommandPalette] 儲存快取失敗: {e.Message}");
            }
        }

        public static List<AssetEntry> LoadCacheFromFile(string cacheFilePath, SearchMode mode)
        {
            try
            {
                if (!File.Exists(cacheFilePath))
                    return null;

                var stopwatch = Stopwatch.StartNew();
                var json = File.ReadAllText(cacheFilePath);
                var cacheData = JsonUtility.FromJson<CacheContainer>(json);

                if (cacheData?.assets == null)
                    return null;

                var assets = new List<AssetEntry>();
                var outdatedAssets = new List<AssetCacheData>();
                var outdatedCount = 0;
                var loadedCount = 0;
                var updatedAssets = new List<AssetEntry>();
                var cacheNeedsUpdate = false;

                foreach (var cachedAsset in cacheData.assets)
                {
                    // 檢查檔案是否仍存在
                    if (!File.Exists(cachedAsset.path))
                    {
                        outdatedCount++;
                        cacheNeedsUpdate = true;
                        continue;
                    }

                    // 檢查檔案修改時間
                    var fileInfo = new FileInfo(cachedAsset.path);
                    if (fileInfo.LastWriteTime.Ticks != cachedAsset.lastModified)
                    {
                        // 檔案已更新，但我們不需要立即載入資源，只需要更新快取資料
                        var updatedCacheData = new AssetCacheData(
                            cachedAsset.name,
                            cachedAsset.path,
                            cachedAsset.guid,
                            cachedAsset.typeName,
                            fileInfo.LastWriteTime.Ticks);
                        
                        var updatedEntry = new AssetEntry(updatedCacheData);
                        assets.Add(updatedEntry);
                        updatedAssets.Add(updatedEntry);
                        loadedCount++;
                        cacheNeedsUpdate = true;
                        Debug.Log($"[CommandPalette] 已更新過期資源快取資料: {cachedAsset.path}");
                        continue;
                    }

                    // 直接從快取資料建立 AssetEntry，不載入實際資源
                    assets.Add(new AssetEntry(cachedAsset));
                    loadedCount++;
                }

                // 如果有過期的資源被更新，重新儲存快取
                if (cacheNeedsUpdate && updatedAssets.Count > 0)
                    try
                    {
                        SaveCacheToFile(assets, cacheFilePath);
                        Debug.Log($"[CommandPalette] 快取已自動更新，包含 {updatedAssets.Count} 個更新的資源");
                    }
                    catch (Exception updateEx)
                    {
                        Debug.LogWarning($"[CommandPalette] 自動更新快取失敗: {updateEx.Message}");
                    }

                stopwatch.Stop();
                var statusMessage = updatedAssets.Count > 0
                    ? $"[CommandPalette] 快取載入完成，有效: {loadedCount} 個，過期: {outdatedCount} 個，已更新: {updatedAssets.Count} 個，耗時 {stopwatch.ElapsedMilliseconds}ms"
                    : $"[CommandPalette] 快取載入完成，有效: {loadedCount} 個，過期: {outdatedCount} 個，耗時 {stopwatch.ElapsedMilliseconds}ms";
                Debug.Log(statusMessage);

                return assets.Count > 0 ? assets : null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CommandPalette] 載入快取失敗: {e.Message}");
                return null;
            }
        }
    }
}
#endif
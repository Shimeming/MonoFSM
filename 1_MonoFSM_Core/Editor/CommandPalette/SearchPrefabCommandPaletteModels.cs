#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CommandPalette
{
    /// <summary>
    ///     搜尋模式枚舉
    /// </summary>
    public enum SearchMode
    {
        Prefabs,
        ScriptableObjects,
        Scenes
    }

    /// <summary>
    ///     資源類型定義
    /// </summary>
    public class AssetTypeDefinition
    {
        public SearchMode Mode { get; set; }
        public string DisplayName { get; set; }
        public string FileExtension { get; set; }
        public string AssetDatabaseFilter { get; set; }
        public System.Type UnityType { get; set; }
        public string CacheFileName { get; set; }

        public AssetTypeDefinition(SearchMode mode, string displayName, string fileExtension, 
            string assetDatabaseFilter, System.Type unityType, string cacheFileName)
        {
            Mode = mode;
            DisplayName = displayName;
            FileExtension = fileExtension;
            AssetDatabaseFilter = assetDatabaseFilter;
            UnityType = unityType;
            CacheFileName = cacheFileName;
        }
    }

    /// <summary>
    ///     資源快取資料結構
    /// </summary>
    [Serializable]
    public class AssetCacheData
    {
        public string name;
        public string path;
        public string guid;
        public string typeName;
        public long lastModified;

        public AssetCacheData(string name, string path, string guid, string typeName, long lastModified)
        {
            this.name = name;
            this.path = path;
            this.guid = guid;
            this.typeName = typeName;
            this.lastModified = lastModified;
        }
    }

    /// <summary>
    ///     快取容器結構
    /// </summary>
    [Serializable]
    public class CacheContainer
    {
        public List<AssetCacheData> assets = new();
        public long cacheTimestamp;
    }

    /// <summary>
    ///     資源條目結構
    /// </summary>
    [Serializable]
    public class AssetEntry
    {
        public string name;
        public string path;
        public string guid;
        public Type assetType;
        public bool iconLoaded;
        
        // 延遲載入的欄位
        private Object _asset;
        private Texture2D _icon;

        // 建構子1：從實際資源建立（用於新掃描）
        public AssetEntry(string name, string path, Object asset)
        {
            this.name = name;
            this.path = path;
            this._asset = asset;
            this.guid = AssetDatabase.AssetPathToGUID(path);
            this.assetType = asset.GetType();
            this.iconLoaded = false;
        }

        // 建構子2：從快取資料建立（用於快取載入）
        public AssetEntry(AssetCacheData cacheData)
        {
            this.name = cacheData.name;
            this.path = cacheData.path;
            this.guid = cacheData.guid;
            this.assetType = Type.GetType(cacheData.typeName);
            this.iconLoaded = false;
            this._asset = null; // 延遲載入
        }

        // 延遲載入資源
        public Object asset
        {
            get
            {
                if (_asset == null)
                {
                    if (assetType == typeof(GameObject))
                        _asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    else if (assetType == typeof(SceneAsset))
                        _asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                    else
                        _asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                }
                return _asset;
            }
        }

        // 延遲載入圖標
        public Texture2D icon
        {
            get
            {
                if (!iconLoaded && _icon == null)
                {
                    var loadedAsset = asset; // 這會觸發延遲載入
                    if (loadedAsset != null)
                    {
                        _icon = AssetPreview.GetMiniThumbnail(loadedAsset);
                        iconLoaded = true;
                    }
                }
                return _icon;
            }
        }

        public void LoadIconIfNeeded()
        {
            if (!iconLoaded && _icon == null)
            {
                var loadedAsset = asset; // 這會觸發延遲載入
                if (loadedAsset != null)
                {
                    _icon = AssetPreview.GetMiniThumbnail(loadedAsset);
                    iconLoaded = true;
                }
            }
        }
    }
}
#endif
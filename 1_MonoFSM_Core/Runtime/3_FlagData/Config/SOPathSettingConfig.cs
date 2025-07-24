using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace MonoFSM.Core
{
    [System.Serializable]
    public class SOPathSetting
    {
        [LabelText("型別名稱")]
        [ReadOnly]
        public string typeName;
        
        [LabelText("目標路徑")]
        [ValueDropdown("GetAvailablePaths")]
        public string targetPath = "Assets";
        
        [LabelText("子路徑")]
        public string subPath = "";

#if UNITY_EDITOR
        [Button("🎯 定位到此路徑", ButtonSizes.Medium)]
        [PropertySpace(5)]
        private void PingPath()
        {
            string fullPath = GetFullPath();
            
            // 嘗試直接載入資源（適用於Assets和有效的Packages路徑）
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(fullPath);
            if (asset != null)
            {
                Debug.Log($"定位到資源: {fullPath}"+asset,asset);
                // Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
                // EditorUtility.FocusProjectWindow();
                return;
            }
            
            // 如果直接載入失敗，嘗試尋找最接近的父資料夾
            string pathToCheck = fullPath;
            while (!string.IsNullOrEmpty(pathToCheck) && pathToCheck != "Assets" && !pathToCheck.StartsWith("Packages/"))
            {
                pathToCheck = System.IO.Path.GetDirectoryName(pathToCheck)?.Replace('\\', '/');
                if (!string.IsNullOrEmpty(pathToCheck))
                {
                    asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(pathToCheck);
                    if (asset != null)
                    {
                        // Selection.activeObject = asset;
                        Debug.Log($"定位到路徑: {pathToCheck}");
                        EditorGUIUtility.PingObject(asset);
                        // EditorUtility.FocusProjectWindow();
                        // Debug.Log($"已定位到最接近的路徑: {pathToCheck}");
                        return;
                    }
                }
            }
            
            Debug.LogWarning($"找不到路徑或其父路徑: {fullPath}");
            
            // 詢問是否創建資料夾
            if (EditorUtility.DisplayDialog("路徑不存在", 
                $"路徑 '{fullPath}' 不存在。\n是否要創建此資料夾？", 
                "創建", "取消"))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(fullPath);
                    AssetDatabase.Refresh();
                    
                    // 創建後再次嘗試定位
                    UnityEngine.Object newAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(fullPath);
                    if (newAsset != null)
                    {
                        EditorGUIUtility.PingObject(newAsset);
                        Debug.Log($"已創建並定位到路徑: {fullPath}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"創建路徑失敗: {e.Message}");
                }
            }
        }


        private string GetFullPath()
        {
            string basePath = targetPath;
            string relativePath = subPath;
            
            // 如果有全域資料夾根名稱，需要加入到相對路徑中
            var config = SOPathSettingConfig.Instance;
            if (config != null && !string.IsNullOrEmpty(config._globalFolderRootName))
            {
                if (string.IsNullOrEmpty(relativePath))
                {
                    relativePath = config._globalFolderRootName;
                }
                else
                {
                    relativePath = config._globalFolderRootName + "/" + relativePath;
                }
            }
            
            if (string.IsNullOrEmpty(relativePath))
            {
                return basePath;
            }
            return System.IO.Path.Combine(basePath, relativePath).Replace('\\', '/');
        }
#endif
        

#if UNITY_EDITOR
        private IEnumerable<string> GetAvailablePaths()
        {
            var paths = new List<string> { "Assets" };
            
            // 取得所有本地套件
            var localPackages = PackageHelper.GetLocalPackagePaths();
            paths.AddRange(localPackages);
            
            return paths;
        }
#endif
    }

    [CreateAssetMenu(fileName = "ScriptableObjectPathConfig", menuName = "Config/ScriptableObject Path Config")]
    public class SOPathSettingConfig : ScriptableObjectSingleton<SOPathSettingConfig>
    {
        [LabelText("預設路徑配置")]
        [ListDrawerSettings(ShowFoldout = true)]
        public List<SOPathSetting> pathSettings = new List<SOPathSetting>();

        /// <summary>
        /// 取得指定型別的基本路徑（Assets 或 Packages/package-name）
        /// </summary>
        /// <param name="type">ScriptableObject 型別</param>
        /// <returns>基本路徑，不含子路徑</returns>
        public string GetBasePathForType(Type type)
        {
            var setting = pathSettings.Find(s => s.typeName == type.Name);
            
            if (setting == null)
            {
                return "Assets"; // 預設回傳 Assets
            }
            
            return setting.targetPath;
        }

        [FormerlySerializedAs("GlobalFolderRootName")] public string _globalFolderRootName = "10_Scriptables";
        /// <summary>
        /// 取得指定型別的相對路徑（子路徑）
        /// </summary>
        /// <param name="type">ScriptableObject 型別</param>
        /// <param name="defaultSubPath">預設子路徑</param>
        /// <returns>相對路徑，如：'Config/ScriptableObjects'</returns>
        public string GetRelativePathForType(Type type, string defaultSubPath = "ScriptableObjects")
        {
            var setting = pathSettings.Find(s => s.typeName == type.Name);
            
            string relativePath;
            if (setting == null)
            {
                // 如果沒有設定，建立預設設定並回傳預設子路徑
                setting = new SOPathSetting
                {
                    typeName = type.Name,
                    targetPath = "Assets",
                    subPath = defaultSubPath
                };
                pathSettings.Add(setting);
                
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
                relativePath = defaultSubPath;
            }
            else
            {
                relativePath = string.IsNullOrEmpty(setting.subPath) ? "" : setting.subPath;
            }

            // 如果 GlobalFolderRootName 存在，將其加到路徑前面
            if (!string.IsNullOrEmpty(_globalFolderRootName))
            {
                if (string.IsNullOrEmpty(relativePath))
                {
                    return _globalFolderRootName;
                }
                return _globalFolderRootName + "/" + relativePath;
            }

            return relativePath;
        }

        /// <summary>
        /// 取得指定型別的創建路徑
        /// </summary>
        /// <param name="type">ScriptableObject 型別</param>
        /// <param name="defaultSubPath">在SOConfig中事先定義的</param>
        /// <returns>完整路徑（包含子路徑）</returns>
        public string GetPathForType(Type type, string defaultSubPath = "ScriptableObjects")
        {
            var basePath = GetBasePathForType(type);
            var relativePath = GetRelativePathForType(type, defaultSubPath);

            if (string.IsNullOrEmpty(relativePath))
            {
                return basePath;
            }

            return Path.Combine(basePath, relativePath).Replace('\\', '/');
        }

        /// <summary>
        /// 設定指定型別的創建路徑
        /// </summary>
        /// <param name="type">ScriptableObject 型別</param>
        /// <param name="targetPath">目標路徑（Assets 或 package 路徑）</param>
        /// <param name="subPath">子路徑</param>
        public void SetPathForType(Type type, string targetPath, string subPath = "")
        {
            var setting = pathSettings.Find(s => s.typeName == type.Name);
            
            if (setting == null)
            {
                setting = new SOPathSetting { typeName = type.Name };
                pathSettings.Add(setting);
            }
            
            setting.targetPath = targetPath;
            setting.subPath = subPath;
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        /// <summary>
        /// 檢查指定路徑是否為有效的創建目標
        /// </summary>
        public bool IsValidPath(string path)
        {
            if (path == "Assets") return true;
            
            // 檢查是否為有效的本地套件路徑
            var localPackages = PackageHelper.GetLocalPackagePaths();
            return localPackages.Contains(path);
        }

#if UNITY_EDITOR
        [Button("重新整理路徑設定")]
        [PropertySpace(10)]
        private void RefreshPathSettings()
        {
            foreach (var setting in pathSettings)
            {
                if (!IsValidPath(setting.targetPath))
                {
                    Debug.LogWarning($"型別 {setting.typeName} 的路徑 {setting.targetPath} 無效，已重置為 Assets");
                    setting.targetPath = "Assets";
                }
            }
            EditorUtility.SetDirty(this);
        }

        [Button("清除無效設定")]
        private void ClearInvalidSettings()
        {
            pathSettings.RemoveAll(s => string.IsNullOrEmpty(s.typeName));
            EditorUtility.SetDirty(this);
        }
#endif
    }
}
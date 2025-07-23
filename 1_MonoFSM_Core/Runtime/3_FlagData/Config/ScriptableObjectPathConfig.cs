using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

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
        [InfoBox("相對於目標路徑的子資料夾，如：'Config/ScriptableObjects'")]
        public string subPath = "";

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
    public class ScriptableObjectPathConfig : ScriptableObjectSingleton<ScriptableObjectPathConfig>
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

        public string GlobalFolderRootName;
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
            if (!string.IsNullOrEmpty(GlobalFolderRootName))
            {
                if (string.IsNullOrEmpty(relativePath))
                {
                    return GlobalFolderRootName;
                }
                return GlobalFolderRootName + "/" + relativePath;
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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System.IO;
#endif

namespace MonoFSM.Core
{
    public static class PackageHelper
    {
#if UNITY_EDITOR
        private static List<string> s_cachedLocalPackages;
        private static bool s_cacheValid = false;

        /// <summary>
        /// 取得所有本地套件的路徑清單（僅限 install from disk 的套件）
        /// </summary>
        public static List<string> GetLocalPackagePaths()
        {
            if (!s_cacheValid || s_cachedLocalPackages == null)
            {
                RefreshLocalPackageCache();
            }
            return s_cachedLocalPackages ?? new List<string>();
        }

        /// <summary>
        /// 重新整理本地套件快取
        /// </summary>
        public static void RefreshLocalPackageCache()
        {
            s_cachedLocalPackages = new List<string>();
            
            try
            {
                var listRequest = Client.List(true, false);
                
                // 等待請求完成
                while (!listRequest.IsCompleted)
                {
                    System.Threading.Thread.Sleep(10);
                }
                
                if (listRequest.Status == StatusCode.Success)
                {
                    foreach (var package in listRequest.Result)
                    {
                        // 只取得本地套件 (source == PackageSource.Local)
                        if (package.source == PackageSource.Local)
                        {
                            var packagePath = package.resolvedPath;
                            if (!string.IsNullOrEmpty(packagePath) && Directory.Exists(packagePath))
                            {
                                // 轉換為相對路徑格式，如: Packages/com.jerryee.unity-mcp
                                var relativePath = GetRelativePackagePath(package.name);
                                s_cachedLocalPackages.Add(relativePath);
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"無法取得套件清單: {listRequest.Error?.message}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"取得本地套件時發生錯誤: {ex.Message}");
                
                // 使用備用方法：直接讀取 manifest.json
                FallbackGetLocalPackages();
            }
            
            s_cacheValid = true;
        }

        /// <summary>
        /// 備用方法：從 manifest.json 直接讀取本地套件
        /// </summary>
        private static void FallbackGetLocalPackages()
        {
            try
            {
                var manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
                if (File.Exists(manifestPath))
                {
                    var manifestText = File.ReadAllText(manifestPath);
                    
                    // 簡單解析 JSON 來找出 file: 開頭的依賴
                    var lines = manifestText.Split('\n');
                    foreach (var line in lines)
                    {
                        if (line.Contains("\"file:") && line.Contains(':'))
                        {
                            var packageNameStart = line.IndexOf('"') + 1;
                            var packageNameEnd = line.IndexOf('"', packageNameStart);
                            if (packageNameEnd > packageNameStart)
                            {
                                var packageName = line.Substring(packageNameStart, packageNameEnd - packageNameStart);
                                s_cachedLocalPackages.Add($"Packages/{packageName}");
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"讀取 manifest.json 時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 將套件名稱轉換為相對路徑
        /// </summary>
        private static string GetRelativePackagePath(string packageName)
        {
            return $"Packages/{packageName}";
        }

        /// <summary>
        /// 檢查指定路徑是否為有效的本地套件路徑
        /// </summary>
        public static bool IsValidLocalPackagePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            if (path == "Assets") return true;
            
            return GetLocalPackagePaths().Contains(path);
        }

        /// <summary>
        /// 取得套件的完整檔案系統路徑
        /// </summary>
        public static string GetPackageFullPath(string packageRelativePath)
        {
            if (packageRelativePath == "Assets")
            {
                return Application.dataPath;
            }
            
            if (packageRelativePath.StartsWith("Packages/"))
            {
                var packageName = packageRelativePath.Substring("Packages/".Length);
                
                // 嘗試從套件管理器取得完整路徑
                try
                {
                    var listRequest = Client.List(true, false);
                    while (!listRequest.IsCompleted)
                    {
                        System.Threading.Thread.Sleep(10);
                    }
                    
                    if (listRequest.Status == StatusCode.Success)
                    {
                        var package = listRequest.Result.FirstOrDefault(p => p.name == packageName);
                        if (package != null && !string.IsNullOrEmpty(package.resolvedPath))
                        {
                            return package.resolvedPath;
                        }
                    }
                }
                catch
                {
                    // Fallback: 組合路徑
                }
                
                // Fallback: 假設在專案根目錄
                return Path.Combine(Application.dataPath, "..", packageRelativePath);
            }
            
            return null;
        }

        /// <summary>
        /// 清除快取
        /// </summary>
        public static void ClearCache()
        {
            s_cachedLocalPackages = null;
            s_cacheValid = false;
        }


#else
        // Runtime 版本 - 只回傳空清單
        public static List<string> GetLocalPackagePaths()
        {
            return new List<string>();
        }

        public static bool IsValidLocalPackagePath(string path)
        {
            return path == "Assets";
        }

        public static string GetPackageFullPath(string packageRelativePath)
        {
            return packageRelativePath == "Assets" ? Application.dataPath : null;
        }
        
        public static void RefreshLocalPackageCache() { }
        public static void ClearCache() { }
#endif
    }
}
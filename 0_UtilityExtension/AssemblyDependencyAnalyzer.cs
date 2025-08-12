using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using Newtonsoft.Json.Linq;
#endif

namespace MonoFSM.Core
{
    /// <summary>
    /// Assembly Definition 依賴分析器
    /// 分析 package 內的 asmdef 引用，並自動更新 package.json dependencies
    /// </summary>
    public static class AssemblyDependencyAnalyzer
    {
        // Static cache for GUID mappings
        private static Dictionary<string, string> s_guidToPackageMap;
        private static Dictionary<string, string> s_guidToAsmdefNameMap;
        private static bool s_guidMappingCacheValid;

        /// <summary>
        /// Assembly 依賴資訊
        /// </summary>
        [System.Serializable]
        public class AssemblyDependencyInfo
        {
            public string assemblyName;
            public string assemblyPath;
            public string packageName;
            public string packagePath;
            public List<string> referencedGUIDs = new List<string>();
            public List<ReferencedPackageInfo> referencedPackages =
                new List<ReferencedPackageInfo>();
            public bool hasExternalReferences;

            public AssemblyDependencyInfo(string name, string path)
            {
                assemblyName = name;
                assemblyPath = path;
                packageName = "";
                packagePath = "";
                hasExternalReferences = false;
            }
        }

        /// <summary>
        /// 被引用的 Package 資訊
        /// </summary>
        [System.Serializable]
        public class ReferencedPackageInfo
        {
            public string packageName;
            public string packagePath;
            public string gitUrl;
            public bool isLocalPackage;
            public bool hasGitUrl;
            public string assemblyName; // 被引用的 assembly 名稱

            public ReferencedPackageInfo(string name)
            {
                packageName = name;
                packagePath = "";
                gitUrl = "";
                isLocalPackage = false;
                hasGitUrl = false;
                assemblyName = "";
            }
        }

        /// <summary>
        /// 分析結果
        /// </summary>
        [System.Serializable]
        public class AnalysisResult
        {
            public string targetPackageJsonPath;
            public string targetPackageName;
            public List<AssemblyDependencyInfo> assemblies = new List<AssemblyDependencyInfo>();
            public List<ReferencedPackageInfo> missingDependencies =
                new List<ReferencedPackageInfo>();
            public List<ReferencedPackageInfo> existingDependencies =
                new List<ReferencedPackageInfo>();
            public List<ReferencedPackageInfo> needGitUrlDependencies =
                new List<ReferencedPackageInfo>();
            public int totalAssemblies;
            public int externalReferences;

            public AnalysisResult(string packageJsonPath)
            {
                targetPackageJsonPath = packageJsonPath;
                targetPackageName = GetPackageNameFromPath(packageJsonPath);
                totalAssemblies = 0;
                externalReferences = 0;
            }

            private string GetPackageNameFromPath(string packageJsonPath)
            {
                try
                {
                    // 嘗試從 package.json 檔案中讀取 name
                    if (File.Exists(packageJsonPath))
                    {
                        var packageText = File.ReadAllText(packageJsonPath);
                        var packageJson = JObject.Parse(packageText);
                        var name = packageJson["name"]?.ToString();
                        if (!string.IsNullOrEmpty(name))
                        {
                            return name;
                        }
                    }

                    // 備用：從目錄名稱取得
                    var packageDir = Path.GetDirectoryName(packageJsonPath);
                    return Path.GetFileName(packageDir);
                }
                catch
                {
                    return "Unknown";
                }
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// 分析指定 package.json 的 Assembly Dependencies
        /// </summary>
        public static AnalysisResult AnalyzePackageDependencies(string packageJsonPath)
        {
            var result = new AnalysisResult(packageJsonPath);

            if (!File.Exists(packageJsonPath))
            {
                Debug.LogError(
                    $"[AssemblyDependencyAnalyzer] package.json 不存在: {packageJsonPath}"
                );
                return result;
            }

            try
            {
                // 讀取目標 package.json
                var packageJson = JObject.Parse(File.ReadAllText(packageJsonPath));
                result.targetPackageName = packageJson["name"]?.ToString() ?? "Unknown";

                // 取得 package 目錄
                var packageDir = Path.GetDirectoryName(packageJsonPath);

                // 尋找該 package 內的所有 asmdef 檔案
                var asmdefFiles = Directory.GetFiles(
                    packageDir,
                    "*.asmdef",
                    SearchOption.AllDirectories
                );
                result.totalAssemblies = asmdefFiles.Length;

                Debug.Log(
                    $"[AssemblyDependencyAnalyzer] 分析 {result.targetPackageName}，找到 {asmdefFiles.Length} 個 asmdef 檔案"
                );

                // 建立 GUID 到 Package 的映射
                BuildGuidToPackageMaps();
                var existingDependencies = GetExistingDependencies(packageJson);

                // 分析每個 asmdef
                foreach (var asmdefPath in asmdefFiles)
                {
                    Debug.Log(
                        $"[AssemblyDependencyAnalyzer] 分析 assembly: {Path.GetFileName(asmdefPath)}"
                    );
                    var assemblyInfo = AnalyzeAssemblyDefinition(
                        asmdefPath,
                        result.targetPackageName
                    );
                    result.assemblies.Add(assemblyInfo);

                    Debug.Log(
                        $"[AssemblyDependencyAnalyzer] {assemblyInfo.assemblyName}: 引用數={assemblyInfo.referencedGUIDs.Count}, 外部引用={assemblyInfo.hasExternalReferences}, 外部package數={assemblyInfo.referencedPackages.Count}"
                    );

                    if (assemblyInfo.hasExternalReferences)
                    {
                        result.externalReferences++;

                        // 檢查引用的 packages
                        foreach (var refPackage in assemblyInfo.referencedPackages)
                        {
                            if (
                                refPackage.packageName != result.targetPackageName
                                && !IsUnityBuiltInPackage(refPackage.packageName)
                                && refPackage.packageName != "Assets"
                            ) // 跳過 Assets，無法作為 package 安裝
                            {
                                // 檢查是否已存在於 dependencies 中
                                if (existingDependencies.ContainsKey(refPackage.packageName))
                                {
                                    refPackage.gitUrl = existingDependencies[
                                        refPackage.packageName
                                    ];
                                    refPackage.hasGitUrl = IsGitUrl(refPackage.gitUrl);

                                    if (
                                        !result.existingDependencies.Any(d =>
                                            d.packageName == refPackage.packageName
                                        )
                                    )
                                    {
                                        result.existingDependencies.Add(refPackage);
                                    }
                                }
                                else
                                {
                                    // 新的依賴
                                    if (
                                        !result.missingDependencies.Any(d =>
                                            d.packageName == refPackage.packageName
                                        )
                                    )
                                    {
                                        result.missingDependencies.Add(refPackage);

                                        // 如果是本地 package，需要提供 Git URL
                                        if (refPackage.isLocalPackage)
                                        {
                                            result.needGitUrlDependencies.Add(refPackage);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                Debug.Log(
                    $"[AssemblyDependencyAnalyzer] 分析完成 - 缺失依賴: {result.missingDependencies.Count}, 需要 Git URL: {result.needGitUrlDependencies.Count}"
                );
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AssemblyDependencyAnalyzer] 分析失敗: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// 分析單個 Assembly Definition
        /// </summary>
        private static AssemblyDependencyInfo AnalyzeAssemblyDefinition(
            string asmdefPath,
            string targetPackageName
        )
        {
            var asmdefName = Path.GetFileNameWithoutExtension(asmdefPath);
            var assemblyInfo = new AssemblyDependencyInfo(asmdefName, asmdefPath);

            try
            {
                var asmdefJson = JObject.Parse(File.ReadAllText(asmdefPath));
                var references = asmdefJson["references"] as JArray;

                if (references != null)
                {
                    foreach (var reference in references)
                    {
                        var rawGuid = reference.ToString();
                        // 移除 "GUID:" 前綴，只保留實際的 GUID
                        var guid = rawGuid.StartsWith("GUID:") ? rawGuid.Substring(5) : rawGuid;
                        assemblyInfo.referencedGUIDs.Add(rawGuid); // 保存原始格式用於記錄
                        var asmdefName_fromGuid = s_guidToAsmdefNameMap?.GetValueOrDefault(
                            guid,
                            "Unknown"
                        );
                        Debug.Log(
                            $"[AssemblyDependencyAnalyzer] <color=blue>{asmdefName_fromGuid}</color> from {asmdefName} 檢查 GUID: {rawGuid} -> 清理後: {guid}, 在映射中: {s_guidToPackageMap?.GetValueOrDefault(guid)}"
                        );

                        if (s_guidToPackageMap != null && s_guidToPackageMap.ContainsKey(guid))
                        {
                            var packageName = s_guidToPackageMap[guid];

                            var packagePath = GetPackagePathByName(packageName);

                            Debug.Log(
                                $"[AssemblyDependencyAnalyzer] {asmdefName} 引用 GUID {guid} -> package: {packageName} (目標package: {targetPackageName})"
                            );

                            // 檢查是否為外部引用（不在目標 package 內）
                            if (
                                packageName != targetPackageName
                                && !string.IsNullOrEmpty(packageName)
                            )
                            {
                                assemblyInfo.hasExternalReferences = true;

                                var refPackageInfo = new ReferencedPackageInfo(packageName)
                                {
                                    packagePath = packagePath,
                                    isLocalPackage = IsLocalPackage(packageName),
                                    assemblyName =
                                        asmdefName_fromGuid ?? GetAssemblyNameByGuid(guid),
                                };

                                assemblyInfo.referencedPackages.Add(refPackageInfo);
                                Debug.Log(
                                    $"[AssemblyDependencyAnalyzer] 添加外部引用: {packageName} (本地package: {refPackageInfo.isLocalPackage})"
                                );
                            }
                            else if (packageName == targetPackageName)
                            {
                                Debug.Log(
                                    $"[AssemblyDependencyAnalyzer] 跳過同package引用: {packageName}"
                                );
                            }
                        }
                        else
                        {
                            Debug.LogWarning(
                                $"[AssemblyDependencyAnalyzer] 無法解析 GUID: {guid} 在 {asmdefName}"
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[AssemblyDependencyAnalyzer] 分析 {asmdefPath} 失敗: {ex.Message}"
                );
            }

            return assemblyInfo;
        }

        /// <summary>
        /// 清除 GUID 映射快取
        /// </summary>
        public static void ClearGuidMappingCache()
        {
            s_guidToPackageMap = null;
            s_guidToAsmdefNameMap = null;
            s_guidMappingCacheValid = false;
        }

        /// <summary>
        /// 建立 GUID 到 Package 名稱的映射（Static 版本）
        /// </summary>
        private static void BuildGuidToPackageMaps()
        {
            // 如果快取已經有效，就不重建
            if (
                s_guidMappingCacheValid
                && s_guidToPackageMap != null
                && s_guidToAsmdefNameMap != null
            )
                return;

            s_guidToPackageMap = new Dictionary<string, string>();
            s_guidToAsmdefNameMap = new Dictionary<string, string>();

            try
            {
                // 使用擴充的 PackageHelper 取得所有 packages
                var allPackages = PackageHelper.GetAllPackages();
                Debug.Log($"[AssemblyDependencyAnalyzer] 找到 {allPackages.Count} 個 packages");

                foreach (var package in allPackages)
                {
                    Debug.Log(
                        $"[AssemblyDependencyAnalyzer] 處理 package: {package.name} (source: {package.source})"
                    );

                    // 取得 package 的完整路徑
                    string packageFullPath = null;
                    if (package.source == UnityEditor.PackageManager.PackageSource.Local)
                    {
                        // 本地 package
                        packageFullPath = PackageHelper.GetPackageFullPath(
                            $"Packages/{package.name}"
                        );
                    }
                    else
                    {
                        // Git 或 Registry packages，使用 resolvedPath
                        packageFullPath = package.resolvedPath;
                    }

                    if (!string.IsNullOrEmpty(packageFullPath) && Directory.Exists(packageFullPath))
                    {
                        // 搜尋該 package 內的所有 asmdef 檔案
                        var asmdefFiles = Directory.GetFiles(
                            packageFullPath,
                            "*.asmdef",
                            SearchOption.AllDirectories
                        );

                        foreach (var asmdefFile in asmdefFiles)
                        {
                            var metaFile = asmdefFile + ".meta";
                            if (File.Exists(metaFile))
                            {
                                var guid = ExtractGuidFromMetaFile(metaFile);
                                if (!string.IsNullOrEmpty(guid))
                                {
                                    s_guidToPackageMap[guid] = package.name;
                                    s_guidToAsmdefNameMap[guid] = Path.GetFileNameWithoutExtension(
                                        asmdefFile
                                    );
                                    // Debug.Log($"[AssemblyDependencyAnalyzer] 映射 package: {guid} -> {package.name} ({Path.GetFileName(asmdefFile)})");
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"[AssemblyDependencyAnalyzer] 無法存取 package 路徑: {package.name} -> {packageFullPath}"
                        );
                    }
                }
                // 2. 處理 Asset的
                var allAsmdefGuids = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset");
                Debug.Log(
                    "[AssemblyDependencyAnalyzer] 找到Asset中 asmdef GUIDs: "
                        + allAsmdefGuids.Length
                );
                foreach (var guid in allAsmdefGuids)
                {
                    // 如果已經在 packages 中處理過，就跳過
                    if (s_guidToPackageMap.ContainsKey(guid))
                        continue;

                    var asmdefPath = AssetDatabase.GUIDToAssetPath(guid);
                    s_guidToPackageMap[guid] = "Assets"; // 預設為主專案
                    s_guidToAsmdefNameMap[guid] = Path.GetFileNameWithoutExtension(asmdefPath);
                }

                Debug.Log(
                    $"[AssemblyDependencyAnalyzer] 建立了 {s_guidToPackageMap.Count} 個 GUID 到 Package 的映射"
                );
                s_guidMappingCacheValid = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AssemblyDependencyAnalyzer] 建立 GUID 映射失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 從 .meta 檔案中提取 GUID
        /// </summary>
        private static string ExtractGuidFromMetaFile(string metaFilePath)
        {
            try
            {
                var lines = File.ReadAllLines(metaFilePath);
                foreach (var line in lines)
                {
                    if (line.StartsWith("guid: "))
                    {
                        return line.Substring(6).Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    $"[AssemblyDependencyAnalyzer] 無法讀取 meta 檔案 {metaFilePath}: {ex.Message}"
                );
            }

            return null;
        }

        /// <summary>
        /// 從 Asset 路徑中取得 Package 名稱
        /// </summary>
        private static string GetPackageNameFromAssetPath(string assetPath)
        {
            if (assetPath.StartsWith("Packages/"))
            {
                var parts = assetPath.Split('/');
                if (parts.Length >= 2)
                {
                    return parts[1]; // Packages/com.example.package/...
                }
            }
            else if (assetPath.StartsWith("Assets/"))
            {
                return ""; // 主專案
            }

            return "";
        }

        /// <summary>
        /// 取得現有的 dependencies
        /// </summary>
        private static Dictionary<string, string> GetExistingDependencies(JObject packageJson)
        {
            var dependencies = new Dictionary<string, string>();

            var depsObject = packageJson["dependencies"] as JObject;
            if (depsObject != null)
            {
                foreach (var dep in depsObject)
                {
                    dependencies[dep.Key] = dep.Value.ToString();
                }
            }

            return dependencies;
        }

        /// <summary>
        /// 根據 package 名稱取得路徑
        /// </summary>
        private static string GetPackagePathByName(string packageName)
        {
            // 使用 PackageHelper 的功能
            var localPackages = PackageHelper.GetLocalPackagePaths();
            var targetPath = $"Packages/{packageName}";

            if (localPackages.Contains(targetPath))
            {
                return PackageHelper.GetPackageFullPath(targetPath);
            }

            return "";
        }

        /// <summary>
        /// 檢查是否為本地 package
        /// </summary>
        private static bool IsLocalPackage(string packageName)
        {
            var localPackages = PackageHelper.GetLocalPackagePaths();
            return localPackages.Any(p => p.EndsWith(packageName));
        }

        /// <summary>
        /// 檢查是否為 Unity 內建 package
        /// </summary>
        private static bool IsUnityBuiltInPackage(string packageName)
        {
            return packageName.StartsWith("com.unity.modules.")
                || packageName.StartsWith("com.unity.")
                || packageName == "";
        }

        /// <summary>
        /// 根據 GUID 取得 Assembly 名稱
        /// </summary>
        private static string GetAssemblyNameByGuid(string guid)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (!string.IsNullOrEmpty(assetPath))
            {
                try
                {
                    var asmdefJson = JObject.Parse(File.ReadAllText(assetPath));
                    return asmdefJson["name"]?.ToString()
                        ?? Path.GetFileNameWithoutExtension(assetPath);
                }
                catch
                {
                    return Path.GetFileNameWithoutExtension(assetPath);
                }
            }
            return "Unknown";
        }

        /// <summary>
        /// 檢查是否為 Git URL
        /// </summary>
        private static bool IsGitUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            return url.StartsWith("https://github.com/")
                || url.StartsWith("git@github.com:")
                || url.StartsWith("git://")
                || url.Contains(".git");
        }

        /// <summary>
        /// 自動更新 package.json dependencies
        /// </summary>
        public static void UpdatePackageJsonDependencies(
            AnalysisResult analysisResult,
            Dictionary<string, string> gitUrlMappings = null
        )
        {
            if (analysisResult.missingDependencies.Count == 0)
            {
                Debug.Log("[AssemblyDependencyAnalyzer] 沒有缺失的 dependencies 需要更新");
                return;
            }

            try
            {
                var packageJson = JObject.Parse(
                    File.ReadAllText(analysisResult.targetPackageJsonPath)
                );
                var dependencies = packageJson["dependencies"] as JObject;
                if (dependencies == null)
                {
                    packageJson["dependencies"] = dependencies = new JObject();
                }

                var addedCount = 0;
                foreach (var missingDep in analysisResult.missingDependencies)
                {
                    if (!dependencies.ContainsKey(missingDep.packageName))
                    {
                        string dependencyUrl;

                        // 嘗試從 Git URL 映射中取得
                        if (
                            gitUrlMappings != null
                            && gitUrlMappings.ContainsKey(missingDep.packageName)
                        )
                        {
                            dependencyUrl = gitUrlMappings[missingDep.packageName];
                        }
                        else if (!string.IsNullOrEmpty(missingDep.gitUrl))
                        {
                            dependencyUrl = missingDep.gitUrl;
                        }
                        else
                        {
                            // 使用版本號或本地路徑
                            dependencyUrl = missingDep.isLocalPackage
                                ? $"file:../{missingDep.packageName}"
                                : "1.0.0"; // 預設版本
                        }

                        dependencies[missingDep.packageName] = dependencyUrl;
                        addedCount++;
                        Debug.Log(
                            $"[AssemblyDependencyAnalyzer] 已添加 dependency: {missingDep.packageName} -> {dependencyUrl}"
                        );
                    }
                }

                if (addedCount > 0)
                {
                    File.WriteAllText(
                        analysisResult.targetPackageJsonPath,
                        packageJson.ToString(Newtonsoft.Json.Formatting.Indented)
                    );
                    Debug.Log(
                        $"[AssemblyDependencyAnalyzer] 已更新 package.json，添加了 {addedCount} 個 dependencies"
                    );
                    AssetDatabase.Refresh();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[AssemblyDependencyAnalyzer] 更新 package.json 失敗: {ex.Message}"
                );
            }
        }

#else
        // Runtime 版本 - 只提供基本功能
        public static AnalysisResult AnalyzePackageDependencies(string packageJsonPath)
        {
            Debug.LogWarning(
                "[AssemblyDependencyAnalyzer] Runtime 模式下無法分析 Assembly Dependencies"
            );
            return new AnalysisResult(packageJsonPath);
        }

        public static void UpdatePackageJsonDependencies(
            AnalysisResult analysisResult,
            Dictionary<string, string> gitUrlMappings = null
        )
        {
            Debug.LogWarning("[AssemblyDependencyAnalyzer] Runtime 模式下無法更新 package.json");
        }
#endif
    }
}

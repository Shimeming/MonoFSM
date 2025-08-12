using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonoFSM.Core;
using UnityEditor;
using UnityEngine;

namespace MonoFSM.Utility.Editor
{
    /// <summary>
    /// Git Dependencies 管理視窗
    /// 提供視覺化的依賴管理界面，包含 Assembly Dependency 分析功能
    /// </summary>
    public class GitDependencyWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private GitDependencyInstaller.DependencyCheckResult checkResult;
        private bool isChecking = false;
        private bool showInstalledDependencies = true;
        private bool showMissingDependencies = true;
        private string searchFilter = "";

        // Assembly Dependency Analysis
        private int currentTab = 0;
        private readonly string[] tabNames = { "Git Dependencies", "Assembly Analysis" };
        private AssemblyDependencyAnalyzer.AnalysisResult assemblyAnalysisResult;
        private string selectedPackageJsonPath = "";
        private bool isAnalyzing = false;
        private Dictionary<string, string> gitUrlInputs = new Dictionary<string, string>();

        // Package selection
        private string[] availablePackageOptions;
        private string[] availablePackagePaths;
        private int selectedPackageIndex = 0;

        // GUI Styles
        private GUIStyle headerStyle;
        private GUIStyle installedStyle;
        private GUIStyle missingStyle;
        private bool stylesInitialized = false;

        [MenuItem("Tools/MonoFSM/Dependencies/管理 Git Dependencies", false, 100)]
        public static GitDependencyWindow ShowWindow()
        {
            var window = GetWindow<GitDependencyWindow>("Git Dependencies");
            window.minSize = new Vector2(600, 400);
            window.Show();
            return window;
        }

        private void OnEnable()
        {
            // 視窗開啟時自動檢查依賴
            RefreshDependencies();
            // 初始化 package 選項
            RefreshPackageOptions();
        }

        private void InitializeStyles()
        {
            if (stylesInitialized)
                return;

            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                margin = new RectOffset(0, 0, 10, 5),
            };

            installedStyle = new GUIStyle(EditorStyles.helpBox)
            {
                normal = { textColor = new Color(0.2f, 0.8f, 0.2f) },
            };

            missingStyle = new GUIStyle(EditorStyles.helpBox)
            {
                normal = { textColor = new Color(0.8f, 0.2f, 0.2f) },
            };

            stylesInitialized = true;
        }

        private void OnGUI()
        {
            InitializeStyles();

            DrawHeader();
            DrawTabs();

            switch (currentTab)
            {
                case 0:
                    DrawGitDependenciesTab();
                    break;
                case 1:
                    DrawAssemblyAnalysisTab();
                    break;
            }
        }

        private void DrawTabs()
        {
            currentTab = GUILayout.Toolbar(currentTab, tabNames);
            GUILayout.Space(10);
        }

        private void DrawGitDependenciesTab()
        {
            DrawToolbar();
            DrawSearchFilter();
            DrawDependenciesList();
            DrawFooter();
        }

        private void DrawAssemblyAnalysisTab()
        {
            DrawAssemblyAnalysisHeader();
            DrawPackageSelector();
            DrawAnalysisResults();
            DrawAssemblyAnalysisFooter();
        }

        private void DrawHeader()
        {
            GUILayout.Space(10);
            GUILayout.Label("MonoFSM Git Dependencies 管理器", headerStyle);

            if (checkResult != null)
            {
                var statusText = checkResult.allDependenciesInstalled
                    ? "✓ 所有依賴已安裝"
                    : $"⚠ {checkResult.missingDependencies.Count} 個依賴缺失";

                var statusColor = checkResult.allDependenciesInstalled ? Color.green : Color.yellow;

                var originalColor = GUI.color;
                GUI.color = statusColor;
                GUILayout.Label(statusText, EditorStyles.boldLabel);
                GUI.color = originalColor;
            }

            GUILayout.Space(10);
        }

        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("重新檢查", EditorStyles.toolbarButton))
            {
                RefreshDependencies();
            }

            GUI.enabled = checkResult != null && !checkResult.allDependenciesInstalled;
            if (GUILayout.Button("安裝所有缺失依賴", EditorStyles.toolbarButton))
            {
                InstallMissingDependencies();
            }
            GUI.enabled = true;

            //FIXME: 這感覺不對
            // if (GUILayout.Button("更新本地 Packages", EditorStyles.toolbarButton))
            // {
            //     GitDependencyManager.UpdateAllLocalPackageDependencies();
            // }

            if (GUILayout.Button("生成報告", EditorStyles.toolbarButton))
            {
                GitDependencyManager.GenerateDependencyReport();
            }

            GUILayout.FlexibleSpace();

            if (isChecking)
            {
                GUILayout.Label("檢查中...", EditorStyles.toolbarButton);
            }

            GUILayout.EndHorizontal();
        }

        private void DrawSearchFilter()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("搜尋:", GUILayout.Width(40));
            searchFilter = GUILayout.TextField(searchFilter);

            if (GUILayout.Button("清除", GUILayout.Width(50)))
            {
                searchFilter = "";
                GUI.FocusControl(null);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            showInstalledDependencies = GUILayout.Toggle(showInstalledDependencies, "顯示已安裝");
            showMissingDependencies = GUILayout.Toggle(showMissingDependencies, "顯示缺失");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
        }

        private void DrawDependenciesList()
        {
            if (checkResult == null)
            {
                GUILayout.Label("尚未檢查依賴。點擊 '重新檢查' 開始。");
                return;
            }

            if (checkResult.gitDependencies.Count == 0)
            {
                GUILayout.Label("沒有找到 Git Dependencies。");
                return;
            }

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            var filteredDependencies = GetFilteredDependencies();

            foreach (var dependency in filteredDependencies)
            {
                DrawDependencyItem(dependency);
            }

            GUILayout.EndScrollView();
        }

        private List<GitDependencyInstaller.GitDependencyInfo> GetFilteredDependencies()
        {
            var filtered = checkResult.gitDependencies.AsEnumerable();

            // 狀態過濾
            if (!showInstalledDependencies)
                filtered = filtered.Where(d => !d.isInstalled);
            if (!showMissingDependencies)
                filtered = filtered.Where(d => d.isInstalled);

            // 搜尋過濾
            if (!string.IsNullOrEmpty(searchFilter))
            {
                var filter = searchFilter.ToLower();
                filtered = filtered.Where(d =>
                    d.packageName.ToLower().Contains(filter) || d.gitUrl.ToLower().Contains(filter)
                );
            }

            return filtered.OrderBy(d => d.isInstalled ? 0 : 1).ThenBy(d => d.packageName).ToList();
        }

        private void DrawDependencyItem(GitDependencyInstaller.GitDependencyInfo dependency)
        {
            var style = dependency.isInstalled ? installedStyle : missingStyle;

            GUILayout.BeginVertical(style);

            // 標題行
            GUILayout.BeginHorizontal();

            var statusIcon = dependency.isInstalled ? "✓" : "✗";
            var statusColor = dependency.isInstalled ? Color.green : Color.red;

            var originalColor = GUI.color;
            GUI.color = statusColor;
            GUILayout.Label(statusIcon, GUILayout.Width(20));
            GUI.color = originalColor;

            GUILayout.Label(dependency.packageName, EditorStyles.boldLabel);

            GUILayout.FlexibleSpace();

            if (!dependency.isInstalled)
            {
                if (GUILayout.Button("安裝", GUILayout.Width(60)))
                {
                    InstallSingleDependency(dependency);
                }
            }
            else if (!string.IsNullOrEmpty(dependency.installedVersion))
            {
                GUILayout.Label($"v{dependency.installedVersion}", EditorStyles.miniLabel);
            }

            GUILayout.EndHorizontal();

            // URL 行
            GUILayout.BeginHorizontal();
            GUILayout.Label("URL:", GUILayout.Width(30));

            if (GUILayout.Button(dependency.gitUrl, EditorStyles.linkLabel))
            {
                // 複製 URL 到剪貼簿
                EditorGUIUtility.systemCopyBuffer = dependency.gitUrl;
                Debug.Log($"已複製到剪貼簿: {dependency.gitUrl}");
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.Space(5);
        }

        private void DrawFooter()
        {
            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("提示:", EditorStyles.boldLabel);
            GUILayout.Label("• 點擊 URL 可複製到剪貼簿");
            GUILayout.Label("• 建議在安裝完成後重新啟動 Unity Editor");
            GUILayout.Label("• 如遇到問題，請查看 Console 的詳細日誌");
            GUILayout.EndVertical();
        }

        private void RefreshDependencies()
        {
            isChecking = true;
            Repaint();

            // 使用 EditorApplication.delayCall 避免在 OnGUI 中執行耗時操作
            EditorApplication.delayCall += () =>
            {
                checkResult = GitDependencyInstaller.CheckGitDependencies();
                isChecking = false;
                Repaint();
            };
        }

        private void InstallMissingDependencies()
        {
            if (checkResult?.missingDependencies?.Count > 0)
            {
                var message =
                    $"確定要安裝 {checkResult.missingDependencies.Count} 個缺失的依賴嗎？\n\n"
                    + "這可能需要一些時間，請耐心等候。";

                if (EditorUtility.DisplayDialog("確認安裝", message, "確定", "取消"))
                {
                    GitDependencyInstaller.InstallMissingGitDependencies();

                    // 安裝完成後重新檢查
                    EditorApplication.delayCall += () =>
                    {
                        RefreshDependencies();
                    };
                }
            }
        }

        private void InstallSingleDependency(GitDependencyInstaller.GitDependencyInfo dependency)
        {
            var message = $"確定要安裝 '{dependency.packageName}' 嗎？\n\nURL: {dependency.gitUrl}";

            if (EditorUtility.DisplayDialog("確認安裝", message, "確定", "取消"))
            {
                Debug.Log($"[GitDependencyWindow] 正在安裝: {dependency.packageName}");

                var addRequest = UnityEditor.PackageManager.Client.Add(dependency.gitUrl);

                // 簡單的等待處理
                EditorApplication.delayCall += () => WaitForInstallation(addRequest, dependency);
            }
        }

        private void WaitForInstallation(
            UnityEditor.PackageManager.Requests.AddRequest request,
            GitDependencyInstaller.GitDependencyInfo dependency
        )
        {
            if (!request.IsCompleted)
            {
                EditorApplication.delayCall += () => WaitForInstallation(request, dependency);
                return;
            }

            if (request.Status == UnityEditor.PackageManager.StatusCode.Success)
            {
                Debug.Log($"[GitDependencyWindow] 成功安裝: {dependency.packageName}");
                EditorUtility.DisplayDialog(
                    "安裝成功",
                    $"'{dependency.packageName}' 已成功安裝！",
                    "確定"
                );
            }
            else
            {
                Debug.LogError(
                    $"[GitDependencyWindow] 安裝失敗: {dependency.packageName} - {request.Error?.message}"
                );
                EditorUtility.DisplayDialog(
                    "安裝失敗",
                    $"'{dependency.packageName}' 安裝失敗。\n\n請查看 Console 獲取詳細錯誤訊息。",
                    "確定"
                );
            }

            // 重新檢查依賴狀態
            RefreshDependencies();
        }

        // ===== Assembly Analysis Tab =====

        private void DrawAssemblyAnalysisHeader()
        {
            GUILayout.Label("Assembly Dependency 分析器", headerStyle);
            GUILayout.Label(
                "分析 package 內的 asmdef 引用，自動更新 dependencies",
                EditorStyles.helpBox
            );
            GUILayout.Space(5);
        }

        private void RefreshPackageOptions()
        {
            var packageOptions = new List<string>();
            var packagePaths = new List<string>();

            try
            {
                // 取得所有 packages
                var allPackages = PackageHelper.GetAllPackages();

                foreach (var package in allPackages)
                {
                    string packageJsonPath = null;

                    if (package.source == UnityEditor.PackageManager.PackageSource.Local)
                    {
                        // 本地 package
                        var packageFullPath = PackageHelper.GetPackageFullPath(
                            $"Packages/{package.name}"
                        );
                        if (!string.IsNullOrEmpty(packageFullPath))
                        {
                            packageJsonPath = Path.Combine(packageFullPath, "package.json");
                        }
                    }
                    else
                    {
                        // Git 或 Registry packages
                        if (!string.IsNullOrEmpty(package.resolvedPath))
                        {
                            packageJsonPath = Path.Combine(package.resolvedPath, "package.json");
                        }
                    }

                    if (!string.IsNullOrEmpty(packageJsonPath) && File.Exists(packageJsonPath))
                    {
                        packageOptions.Add($"{package.displayName} ({package.name})");
                        packagePaths.Add(packageJsonPath);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning(
                    $"[GitDependencyWindow] 取得 package 選項時發生錯誤: {ex.Message}"
                );
            }

            availablePackageOptions = packageOptions.ToArray();
            availablePackagePaths = packagePaths.ToArray();

            // 確保選擇的索引有效
            if (selectedPackageIndex >= availablePackageOptions.Length)
            {
                selectedPackageIndex = 0;
            }

            // 更新選中的路徑
            if (
                availablePackagePaths.Length > 0
                && selectedPackageIndex < availablePackagePaths.Length
            )
            {
                selectedPackageJsonPath = availablePackagePaths[selectedPackageIndex];
            }
        }

        private void DrawPackageSelector()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("選擇 package:", GUILayout.Width(100));

            // 重新整理按鈕
            if (GUILayout.Button("🔄", GUILayout.Width(25)))
            {
                RefreshPackageOptions();
            }

            // 下拉選單
            if (availablePackageOptions != null && availablePackageOptions.Length > 0)
            {
                var newIndex = EditorGUILayout.Popup(selectedPackageIndex, availablePackageOptions);
                if (newIndex != selectedPackageIndex)
                {
                    selectedPackageIndex = newIndex;
                    selectedPackageJsonPath = availablePackagePaths[selectedPackageIndex];
                    assemblyAnalysisResult = null; // 清除舊結果
                }
            }
            else
            {
                GUILayout.Label("沒有可用的 packages", EditorStyles.helpBox);
            }

            // 分析按鈕
            GUI.enabled = !string.IsNullOrEmpty(selectedPackageJsonPath) && !isAnalyzing;
            if (GUILayout.Button("分析", GUILayout.Width(60)))
            {
                AnalyzeSelectedPackage();
            }
            GUI.enabled = true;

            GUILayout.EndHorizontal();

            // 顯示選中的路徑
            if (!string.IsNullOrEmpty(selectedPackageJsonPath))
            {
                GUILayout.Label($"路徑: {selectedPackageJsonPath}", EditorStyles.miniLabel);
            }

            GUILayout.Space(10);
        }

        private void DrawAnalysisResults()
        {
            if (isAnalyzing)
            {
                GUILayout.Label("正在分析中...", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            if (assemblyAnalysisResult == null)
            {
                GUILayout.Label(
                    "請選擇 package.json 並執行分析",
                    EditorStyles.centeredGreyMiniLabel
                );
                return;
            }

            // 結果摘要
            DrawAnalysisSummary();

            GUILayout.Space(10);

            // 缺失的 Dependencies
            if (assemblyAnalysisResult.missingDependencies.Count > 0)
            {
                DrawMissingDependencies();
            }

            // 需要 Git URL 的 Dependencies
            if (assemblyAnalysisResult.needGitUrlDependencies.Count > 0)
            {
                DrawGitUrlInputs();
            }

            // Assembly 詳細資訊
            DrawAssemblyDetails();
        }

        private void DrawAnalysisSummary()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("分析結果", EditorStyles.boldLabel);

            GUILayout.Label($"Package: {assemblyAnalysisResult.targetPackageName}");
            GUILayout.Label($"總計 Assemblies: {assemblyAnalysisResult.totalAssemblies}");
            GUILayout.Label($"有外部引用: {assemblyAnalysisResult.externalReferences}");
            GUILayout.Label(
                $"缺失 Dependencies: {assemblyAnalysisResult.missingDependencies.Count}"
            );
            GUILayout.Label(
                $"已存在 Dependencies: {assemblyAnalysisResult.existingDependencies.Count}"
            );

            GUILayout.EndVertical();
        }

        private void DrawMissingDependencies()
        {
            GUILayout.Label("缺失的 Dependencies:", EditorStyles.boldLabel);

            GUILayout.BeginVertical(EditorStyles.helpBox);
            foreach (var missing in assemblyAnalysisResult.missingDependencies)
            {
                GUILayout.BeginHorizontal();

                var statusColor = missing.isLocalPackage ? Color.yellow : Color.red;
                var originalColor = GUI.color;
                GUI.color = statusColor;
                GUILayout.Label(missing.isLocalPackage ? "○" : "✗", GUILayout.Width(20));
                GUI.color = originalColor;

                GUILayout.Label(missing.packageName);

                if (!string.IsNullOrEmpty(missing.assemblyName))
                {
                    GUILayout.Label($"({missing.assemblyName})", EditorStyles.miniLabel);
                }

                GUILayout.FlexibleSpace();

                if (missing.isLocalPackage)
                {
                    GUILayout.Label("需要 Git URL", EditorStyles.miniLabel);
                }

                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            GUI.enabled = assemblyAnalysisResult.missingDependencies.Count > 0;
            if (GUILayout.Button("更新到 package.json"))
            {
                UpdatePackageJsonWithMissingDeps();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
        }

        private void DrawGitUrlInputs()
        {
            GUILayout.Label("請為本地 packages 提供 Git URLs:", EditorStyles.boldLabel);

            GUILayout.BeginVertical(EditorStyles.helpBox);

            foreach (var needGitUrl in assemblyAnalysisResult.needGitUrlDependencies)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(needGitUrl.packageName, GUILayout.Width(200));

                if (!gitUrlInputs.ContainsKey(needGitUrl.packageName))
                {
                    gitUrlInputs[needGitUrl.packageName] = "";
                }

                gitUrlInputs[needGitUrl.packageName] = GUILayout.TextField(
                    gitUrlInputs[needGitUrl.packageName]
                );

                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("使用 Git URLs 更新"))
            {
                UpdatePackageJsonWithGitUrls();
            }

            if (GUILayout.Button("清空輸入"))
            {
                gitUrlInputs.Clear();
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.Space(10);
        }

        private void DrawAssemblyDetails()
        {
            if (assemblyAnalysisResult.assemblies.Count == 0)
                return;

            GUILayout.Label("Assembly 詳細資訊:", EditorStyles.boldLabel);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, EditorStyles.helpBox);

            foreach (var assembly in assemblyAnalysisResult.assemblies)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);

                GUILayout.BeginHorizontal();
                var statusIcon = assembly.hasExternalReferences ? "↗" : "○";
                var statusColor = assembly.hasExternalReferences ? Color.yellow : Color.green;

                var originalColor = GUI.color;
                GUI.color = statusColor;
                GUILayout.Label(statusIcon, GUILayout.Width(20));
                GUI.color = originalColor;

                GUILayout.Label(assembly.assemblyName, EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                GUILayout.Label($"{assembly.referencedGUIDs.Count} refs", EditorStyles.miniLabel);
                GUILayout.EndHorizontal();

                if (assembly.hasExternalReferences && assembly.referencedPackages.Count > 0)
                {
                    GUILayout.Space(5);
                    foreach (var refPackage in assembly.referencedPackages)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        GUILayout.Label(
                            $"refPackage.packageName → {refPackage.packageName}",
                            EditorStyles.miniLabel
                        );
                        if (!string.IsNullOrEmpty(refPackage.assemblyName))
                        {
                            GUILayout.Label(
                                $"refPackage.assemblyName:({refPackage.assemblyName})",
                                EditorStyles.miniLabel
                            );
                        }
                        GUILayout.EndHorizontal();
                    }
                }

                GUILayout.EndVertical();
                GUILayout.Space(2);
            }

            GUILayout.EndScrollView();
        }

        private void DrawAssemblyAnalysisFooter()
        {
            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Assembly Analysis 說明:", EditorStyles.boldLabel);
            GUILayout.Label("• ○ = 無外部引用");
            GUILayout.Label("• ↗ = 有外部引用");
            GUILayout.Label("• ✗ = 缺失依賴");
            GUILayout.Label("• ○ = 本地依賴（需 Git URL）");
            GUILayout.EndVertical();
        }

        private void AnalyzeSelectedPackage()
        {
            if (string.IsNullOrEmpty(selectedPackageJsonPath))
                return;

            isAnalyzing = true;
            Repaint();

            EditorApplication.delayCall += () =>
            {
                assemblyAnalysisResult = AssemblyDependencyAnalyzer.AnalyzePackageDependencies(
                    selectedPackageJsonPath
                );
                isAnalyzing = false;
                gitUrlInputs.Clear(); // 清空之前的輸入
                Repaint();
            };
        }

        private void UpdatePackageJsonWithMissingDeps()
        {
            if (
                assemblyAnalysisResult == null
                || assemblyAnalysisResult.missingDependencies.Count == 0
            )
                return;

            var message =
                $"確定要將 {assemblyAnalysisResult.missingDependencies.Count} 個缺失的依賴添加到 package.json 嗎？\n\n";
            message += "注意：本地 packages 需要提供 Git URL 才能正確安裝。";

            if (EditorUtility.DisplayDialog("確認更新", message, "確定", "取消"))
            {
                AssemblyDependencyAnalyzer.UpdatePackageJsonDependencies(assemblyAnalysisResult);
                EditorUtility.DisplayDialog(
                    "更新完成",
                    "package.json 已更新！請檢查並提供必要的 Git URLs。",
                    "確定"
                );
            }
        }

        private void UpdatePackageJsonWithGitUrls()
        {
            if (assemblyAnalysisResult == null || gitUrlInputs.Count == 0)
                return;

            // 檢查是否有空的 Git URL
            var emptyUrls = gitUrlInputs
                .Where(kvp => string.IsNullOrWhiteSpace(kvp.Value))
                .ToList();
            if (emptyUrls.Count > 0)
            {
                var emptyPackages = string.Join(", ", emptyUrls.Select(kvp => kvp.Key));
                EditorUtility.DisplayDialog(
                    "輸入不完整",
                    $"以下 packages 缺少 Git URL:\n{emptyPackages}",
                    "確定"
                );
                return;
            }

            AssemblyDependencyAnalyzer.UpdatePackageJsonDependencies(
                assemblyAnalysisResult,
                gitUrlInputs
            );
            EditorUtility.DisplayDialog("更新完成", "package.json 已使用 Git URLs 更新！", "確定");

            // 重新分析以更新狀態
            AnalyzeSelectedPackage();
        }
    }
}

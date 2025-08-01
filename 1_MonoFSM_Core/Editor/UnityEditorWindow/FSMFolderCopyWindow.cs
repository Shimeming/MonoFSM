using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MonoFSM.Editor
{
    public class FSMFolderCopyWindow : EditorWindow
    {
        private FSMFolderCopyTool.FSMFolderData _sourceFolder;
        private FSMFolderCopyTool.CopyOptions _copyOptions = new FSMFolderCopyTool.CopyOptions();
        private Vector2 _scrollPosition;
        private string _selectedFolderPath = "";

        [MenuItem("Tools/MonoFSM/FSM資料夾複製工具")]
        public static void ShowWindow()
        {
            var window = GetWindow<FSMFolderCopyWindow>("FSM資料夾複製工具");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }

        public static void ShowWindow(string folderPath)
        {
            var window = GetWindow<FSMFolderCopyWindow>("FSM資料夾複製工具");
            window.minSize = new Vector2(400, 500);
            window._selectedFolderPath = folderPath;
            
            // 自動填入目標資料夾路徑（來源資料夾的父目錄）
            var parentDirectory = Path.GetDirectoryName(folderPath);
            if (!string.IsNullOrEmpty(parentDirectory))
            {
                window._copyOptions.targetFolderPath = parentDirectory.Replace('\\', '/');
            }
            
            // 設定預設基礎名稱為"未命名"
            window._copyOptions.newFolderBaseName = "未命名";
            
            window.AnalyzeSelectedFolder();
            window.Show();
        }

        private void OnEnable()
        {
            if (!string.IsNullOrEmpty(_selectedFolderPath))
            {
                AnalyzeSelectedFolder();
            }
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            DrawHeader();
            DrawSourceFolderSection();
            
            if (_sourceFolder != null && _sourceFolder.IsValidFSMFolder)
            {
                DrawCopyOptionsSection();
                DrawPreviewSection();
                DrawActionButtons();
            }
            
            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(10);
            
            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            
            EditorGUILayout.LabelField("FSM資料夾複製工具", titleStyle);
            EditorGUILayout.Space(5);
            
            EditorGUILayout.HelpBox(
                "此工具可以複製包含FSM Prefab、Animator Controller和Animation Clips的完整資料夾，" +
                "並自動維護它們之間的引用關係。",
                MessageType.Info);
            
            EditorGUILayout.Space(10);
        }

        private void DrawSourceFolderSection()
        {
            EditorGUILayout.LabelField("來源資料夾", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            EditorGUI.BeginChangeCheck();
            _selectedFolderPath = EditorGUILayout.TextField("資料夾路徑", _selectedFolderPath);
            if (EditorGUI.EndChangeCheck())
            {
                AnalyzeSelectedFolder();
            }
            
            if (GUILayout.Button("選擇", GUILayout.Width(60)))
            {
                var path = EditorUtility.OpenFolderPanel("選擇FSM資料夾", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith(Application.dataPath))
                    {
                        _selectedFolderPath = "Assets" + path.Substring(Application.dataPath.Length);
                        AnalyzeSelectedFolder();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("錯誤", "請選擇項目內的資料夾", "確定");
                    }
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (_sourceFolder != null)
            {
                DrawFolderAnalysisResult();
            }
        }

        private void DrawFolderAnalysisResult()
        {
            EditorGUILayout.Space(5);
            
            if (_sourceFolder.IsValidFSMFolder)
            {
                EditorGUILayout.HelpBox("✓ 檢測到有效的FSM資料夾", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("✗ 此資料夾不包含有效的FSM資源", MessageType.Warning);
            }
            
            // 顯示資料夾內容分析
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("資料夾內容分析:", EditorStyles.miniBoldLabel);
            
            EditorGUILayout.LabelField($"• Prefab檔案: {_sourceFolder.prefabPaths.Count}個");
            EditorGUILayout.LabelField($"• Animator Controller: {_sourceFolder.animatorPaths.Count}個");
            EditorGUILayout.LabelField($"• 動畫檔案: {_sourceFolder.animationPaths.Count}個");
            EditorGUILayout.LabelField($"• 其他資產: {_sourceFolder.otherAssetPaths.Count}個");
            
            EditorGUILayout.EndVertical();
        }

        private void DrawCopyOptionsSection()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("複製選項", EditorStyles.boldLabel);
            
            // 目標資料夾選擇
            EditorGUILayout.BeginHorizontal();
            _copyOptions.targetFolderPath = EditorGUILayout.TextField("目標資料夾", _copyOptions.targetFolderPath);
            
            if (GUILayout.Button("選擇", GUILayout.Width(60)))
            {
                var path = EditorUtility.OpenFolderPanel("選擇目標資料夾", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith(Application.dataPath))
                    {
                        _copyOptions.targetFolderPath = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("錯誤", "請選擇項目內的資料夾", "確定");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // 命名設定
            EditorGUILayout.LabelField("命名設定", EditorStyles.miniBoldLabel);
            _copyOptions.newFolderBaseName = EditorGUILayout.TextField("新基礎名稱", _copyOptions.newFolderBaseName);
            
            if (_sourceFolder != null && !string.IsNullOrEmpty(_copyOptions.newFolderBaseName))
            {
                EditorGUILayout.HelpBox($"會將檔案名稱中的 '{_sourceFolder.folderName}' 替換為 '{_copyOptions.newFolderBaseName}'", MessageType.Info);
                EditorGUILayout.LabelField("範例預覽:");
                EditorGUILayout.LabelField($"  原始: General FSM Variant - {_sourceFolder.folderName}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"  新名: General FSM Variant - {_copyOptions.newFolderBaseName}", EditorStyles.miniLabel);
            }
            else if (_sourceFolder != null)
            {
                EditorGUILayout.HelpBox("請輸入新的基礎名稱，將會替換檔案名稱中的原資料夾名稱", MessageType.Warning);
            }
            
            EditorGUILayout.Space(10);
            
            // Prefab複製選項
            EditorGUILayout.LabelField("Prefab複製方式", EditorStyles.miniBoldLabel);
            _copyOptions.prefabMode = (FSMFolderCopyTool.CopyOptions.PrefabCopyMode)
                EditorGUILayout.EnumPopup("複製模式", _copyOptions.prefabMode);
            
            if (_copyOptions.prefabMode == FSMFolderCopyTool.CopyOptions.PrefabCopyMode.DirectCopy)
            {
                if (_copyOptions.animatorMode == FSMFolderCopyTool.CopyOptions.AnimatorCopyMode.CreateOverrideController)
                {
                    EditorGUILayout.HelpBox("⚠️ 不建議: 當Animator使用Override Controller時，Prefab建議使用Variant模式以保持引用關係的一致性", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox("直接複製: 建立完全獨立的Prefab副本", MessageType.Info);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("建立Variant: 新Prefab將繼承原始Prefab，只覆蓋差異部分", MessageType.Info);
            }
            
            EditorGUILayout.Space(5);
            
            // Animator複製選項
            EditorGUILayout.LabelField("Animator複製方式", EditorStyles.miniBoldLabel);
            _copyOptions.animatorMode = (FSMFolderCopyTool.CopyOptions.AnimatorCopyMode)
                EditorGUILayout.EnumPopup("複製模式", _copyOptions.animatorMode);
            
            if (_copyOptions.animatorMode == FSMFolderCopyTool.CopyOptions.AnimatorCopyMode.DirectCopy)
            {
                EditorGUILayout.HelpBox("直接複製: 建立完全獨立的Animator Controller", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("建立Override Controller: 使用Animator Override Controller，可以單獨替換動畫", MessageType.Info);
            }
            
        }

        private void DrawPreviewSection()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("預覽結果", EditorStyles.boldLabel);
            
            if (string.IsNullOrEmpty(_copyOptions.targetFolderPath))
            {
                EditorGUILayout.HelpBox("請選擇目標資料夾", MessageType.Warning);
                return;
            }
            
            if (string.IsNullOrEmpty(_copyOptions.newFolderBaseName))
            {
                EditorGUILayout.HelpBox("請輸入新基礎名稱", MessageType.Warning);
                return;
            }
            
            var newFolderName = _copyOptions.newFolderBaseName;
            var targetPath = Path.Combine(_copyOptions.targetFolderPath, newFolderName);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"目標路徑: {targetPath}", EditorStyles.miniBoldLabel);
            
            if (AssetDatabase.IsValidFolder(targetPath))
            {
                EditorGUILayout.HelpBox("⚠️ 目標資料夾已存在，複製將會失敗", MessageType.Error);
            }
            else
            {
                EditorGUILayout.LabelField("將會建立的檔案:");
                
                foreach (var prefabPath in _sourceFolder.prefabPaths)
                {
                    var fileName = Path.GetFileNameWithoutExtension(prefabPath);
                    var newName = GetPreviewFileName(fileName);
                    EditorGUILayout.LabelField($"• {newName}.prefab ({GetPrefabModeText()})");
                }
                
                foreach (var animatorPath in _sourceFolder.animatorPaths)
                {
                    var fileName = Path.GetFileNameWithoutExtension(animatorPath);
                    var newName = GetPreviewFileName(fileName);
                    var extension = _copyOptions.animatorMode == FSMFolderCopyTool.CopyOptions.AnimatorCopyMode.CreateOverrideController 
                        ? " Override.overrideController" : ".controller";
                    EditorGUILayout.LabelField($"• {newName}{extension} ({GetAnimatorModeText()})");
                }
                
                foreach (var animPath in _sourceFolder.animationPaths)
                {
                    var fileName = Path.GetFileNameWithoutExtension(animPath);
                    var newName = GetPreviewFileName(fileName);
                    EditorGUILayout.LabelField($"• {newName}.anim");
                }
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.Space(15);
            
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.FlexibleSpace();
            
            var canCopy = _sourceFolder != null && 
                         _sourceFolder.IsValidFSMFolder && 
                         !string.IsNullOrEmpty(_copyOptions.targetFolderPath) &&
                         !string.IsNullOrEmpty(_copyOptions.newFolderBaseName);
            
            EditorGUI.BeginDisabledGroup(!canCopy);
            
            if (GUILayout.Button("開始複製", GUILayout.Width(100), GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("確認複製", 
                    $"確定要複製FSM資料夾到目標位置嗎？\n\n來源: {_sourceFolder.folderPath}\n目標: {_copyOptions.targetFolderPath}", 
                    "確定", "取消"))
                {
                    PerformCopy();
                }
            }
            
            EditorGUI.EndDisabledGroup();
            
            if (GUILayout.Button("重設", GUILayout.Width(60), GUILayout.Height(30)))
            {
                ResetOptions();
            }
            
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.EndHorizontal();
        }

        private void AnalyzeSelectedFolder()
        {
            if (string.IsNullOrEmpty(_selectedFolderPath))
            {
                _sourceFolder = null;
                return;
            }
            
            _sourceFolder = FSMFolderCopyTool.AnalyzeFolder(_selectedFolderPath);
            Repaint();
        }

        private string GetPreviewFileName(string originalName)
        {
            if (string.IsNullOrEmpty(_copyOptions.newFolderBaseName))
                return originalName;
                
            var newName = originalName;
            
            if (_sourceFolder != null)
            {
                // 檢查原始檔名是否包含原資料夾名稱（大小寫不敏感）
                var index = newName.IndexOf(_sourceFolder.folderName, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    var beforePart = newName.Substring(0, index);
                    var afterPart = newName.Substring(index + _sourceFolder.folderName.Length);
                    newName = beforePart + _copyOptions.newFolderBaseName + afterPart;
                    return newName;
                }
                
                // 如果沒有匹配，則添加新基礎名稱作為前綴
                newName = _copyOptions.newFolderBaseName + " " + originalName;
            }
                
            return newName;
        }

        private string GetPrefabModeText()
        {
            return _copyOptions.prefabMode == FSMFolderCopyTool.CopyOptions.PrefabCopyMode.DirectCopy 
                ? "直接複製" : "Variant";
        }

        private string GetAnimatorModeText()
        {
            return _copyOptions.animatorMode == FSMFolderCopyTool.CopyOptions.AnimatorCopyMode.DirectCopy 
                ? "直接複製" : "Override Controller";
        }

        private void PerformCopy()
        {
            try
            {
                EditorUtility.DisplayProgressBar("複製FSM資料夾", "正在複製...", 0.5f);
                
                var success = FSMFolderCopyTool.CopyFSMFolder(_sourceFolder, _copyOptions);
                
                EditorUtility.ClearProgressBar();
                
                if (success)
                {
                    EditorUtility.DisplayDialog("成功", "FSM資料夾複製完成！", "確定");
                    
                    // 選中新建立的資料夾
                    var newFolderName = _copyOptions.newFolderBaseName;
                    var targetPath = Path.Combine(_copyOptions.targetFolderPath, newFolderName);
                    var folderAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(targetPath);
                    
                    if (folderAsset != null)
                    {
                        Selection.activeObject = folderAsset;
                        EditorGUIUtility.PingObject(folderAsset);
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("錯誤", "複製失敗，請檢查Console輸出", "確定");
                }
            }
            catch (System.Exception ex)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("錯誤", $"複製過程中發生錯誤:\n{ex.Message}", "確定");
                Debug.LogError($"FSM資料夾複製錯誤: {ex}");
            }
        }

        private void ResetOptions()
        {
            _copyOptions = new FSMFolderCopyTool.CopyOptions();
            _selectedFolderPath = "";
            _sourceFolder = null;
            Repaint();
        }
    }
}
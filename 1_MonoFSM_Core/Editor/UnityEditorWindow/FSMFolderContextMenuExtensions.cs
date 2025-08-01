using UnityEngine;
using UnityEditor;
using System.IO;

namespace MonoFSM.Editor
{
    public static class FSMFolderContextMenuExtensions
    {
        [MenuItem("Assets/MonoFSM/複製FSM資料夾", false, 2000)]
        private static void CopyFSMFolder()
        {
            var selectedObject = Selection.activeObject;
            if (selectedObject == null) return;

            var assetPath = AssetDatabase.GetAssetPath(selectedObject);
            
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                var folderData = FSMFolderCopyTool.AnalyzeFolder(assetPath);
                
                if (folderData != null && folderData.IsValidFSMFolder)
                {
                    FSMFolderCopyWindow.ShowWindow(assetPath);
                }
                else
                {
                    EditorUtility.DisplayDialog("無效的FSM資料夾", 
                        "選中的資料夾不包含有效的FSM資源。\n\n" +
                        "FSM資料夾應該包含:\n" +
                        "• 至少一個 .prefab 檔案\n" +
                        "• 至少一個 .controller 檔案", 
                        "確定");
                }
            }
        }

        [MenuItem("Assets/MonoFSM/複製FSM資料夾", true)]
        private static bool ValidateCopyFSMFolder()
        {
            var selectedObject = Selection.activeObject;
            if (selectedObject == null) return false;

            var assetPath = AssetDatabase.GetAssetPath(selectedObject);
            return AssetDatabase.IsValidFolder(assetPath);
        }

        [MenuItem("Assets/MonoFSM/分析FSM資料夾", false, 2001)]
        private static void AnalyzeFSMFolder()
        {
            var selectedObject = Selection.activeObject;
            if (selectedObject == null) return;

            var assetPath = AssetDatabase.GetAssetPath(selectedObject);
            
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                var folderData = FSMFolderCopyTool.AnalyzeFolder(assetPath);
                
                if (folderData != null)
                {
                    ShowFolderAnalysisDialog(folderData);
                }
                else
                {
                    EditorUtility.DisplayDialog("錯誤", "無法分析選中的資料夾", "確定");
                }
            }
        }

        [MenuItem("Assets/MonoFSM/分析FSM資料夾", true)]
        private static bool ValidateAnalyzeFSMFolder()
        {
            var selectedObject = Selection.activeObject;
            if (selectedObject == null) return false;

            var assetPath = AssetDatabase.GetAssetPath(selectedObject);
            return AssetDatabase.IsValidFolder(assetPath);
        }

        private static void ShowFolderAnalysisDialog(FSMFolderCopyTool.FSMFolderData folderData)
        {
            var message = $"資料夾分析結果:\n\n";
            message += $"資料夾: {folderData.folderName}\n";
            message += $"路徑: {folderData.folderPath}\n\n";
            
            message += $"Prefab檔案 ({folderData.prefabPaths.Count}個):\n";
            foreach (var path in folderData.prefabPaths)
            {
                message += $"  • {Path.GetFileName(path)}\n";
            }
            
            message += $"\nAnimator Controller ({folderData.animatorPaths.Count}個):\n";
            foreach (var path in folderData.animatorPaths)
            {
                message += $"  • {Path.GetFileName(path)}\n";
            }
            
            message += $"\n動畫檔案 ({folderData.animationPaths.Count}個):\n";
            foreach (var path in folderData.animationPaths)
            {
                message += $"  • {Path.GetFileName(path)}\n";
            }
            
            if (folderData.otherAssetPaths.Count > 0)
            {
                message += $"\n其他資產 ({folderData.otherAssetPaths.Count}個):\n";
                foreach (var path in folderData.otherAssetPaths)
                {
                    message += $"  • {Path.GetFileName(path)}\n";
                }
            }
            
            message += $"\n{(folderData.IsValidFSMFolder ? "✓ 這是一個有效的FSM資料夾" : "✗ 這不是有效的FSM資料夾")}";
            
            EditorUtility.DisplayDialog("FSM資料夾分析", message, "確定");
        }

    }
}
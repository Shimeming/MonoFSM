using UnityEditor;
using UnityEngine;

namespace MonoFSM.Editor
{
    /// <summary>
    /// Project 視窗右鍵選單：複製 Prefab 為文字
    /// </summary>
    public static class PrefabToTextContextMenu
    {
        [MenuItem("Assets/MonoFSM/複製 Prefab 為文字", false, 2100)]
        private static void CopyPrefabAsText()
        {
            var selectedObject = Selection.activeObject;
            if (selectedObject == null) return;

            var assetPath = AssetDatabase.GetAssetPath(selectedObject);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefab == null)
            {
                Debug.LogWarning("無法載入 Prefab");
                return;
            }

            var settings = PrefabExportSettings.CreateQuickCopy();
            var text = PrefabToTextExporter.Export(prefab, settings);

            EditorGUIUtility.systemCopyBuffer = text;
            Debug.Log($"已複製 Prefab \"{prefab.name}\" 為文字 ({text.Length} 字元)");
        }

        [MenuItem("Assets/MonoFSM/複製 Prefab 為文字", true)]
        private static bool ValidateCopyPrefabAsText()
        {
            var selectedObject = Selection.activeObject;
            if (selectedObject == null) return false;

            var assetPath = AssetDatabase.GetAssetPath(selectedObject);
            return assetPath.EndsWith(".prefab");
        }

        [MenuItem("Assets/MonoFSM/複製 Prefab 為文字 (完整)", false, 2101)]
        private static void CopyPrefabAsTextFull()
        {
            var selectedObject = Selection.activeObject;
            if (selectedObject == null) return;

            var assetPath = AssetDatabase.GetAssetPath(selectedObject);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefab == null)
            {
                Debug.LogWarning("無法載入 Prefab");
                return;
            }

            // 完整匯出設定
            var settings = new PrefabExportSettings
            {
                _excludeDefaults = false,
                _onlyPublicFields = false,
                _excludeDefaultTransform = false,
                _includeComments = true
            };

            var text = PrefabToTextExporter.Export(prefab, settings);

            EditorGUIUtility.systemCopyBuffer = text;
            Debug.Log($"已複製 Prefab \"{prefab.name}\" 完整內容為文字 ({text.Length} 字元)");
        }

        [MenuItem("Assets/MonoFSM/複製 Prefab 為文字 (完整)", true)]
        private static bool ValidateCopyPrefabAsTextFull()
        {
            return ValidateCopyPrefabAsText();
        }

        [MenuItem("Assets/MonoFSM/開啟 Prefab 文字匯出器", false, 2102)]
        private static void OpenPrefabTextExporter()
        {
            PrefabToTextWindow.ShowWindow();
        }

        [MenuItem("Assets/MonoFSM/開啟 Prefab 文字匯出器", true)]
        private static bool ValidateOpenPrefabTextExporter()
        {
            return ValidateCopyPrefabAsText();
        }
    }
}

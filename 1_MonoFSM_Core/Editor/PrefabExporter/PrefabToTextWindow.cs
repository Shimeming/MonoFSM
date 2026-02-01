using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace MonoFSM.Editor
{
    /// <summary>
    /// Prefab 文字匯出器進階視窗
    /// </summary>
    public class PrefabToTextWindow : OdinEditorWindow
    {
        [MenuItem("Tools/MonoFSM/Prefab Text Exporter")]
        public static void ShowWindow()
        {
            var window = GetWindow<PrefabToTextWindow>();
            window.titleContent = new GUIContent("Prefab Text Exporter");
            window.minSize = new Vector2(500, 600);
            window.Show();

            // 自動載入選中的 Prefab
            if (Selection.activeObject != null)
            {
                var path = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (path.EndsWith(".prefab"))
                {
                    window._targetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    window.RefreshPreview();
                }
            }
        }

        [Title("目標 Prefab")]
        [Required]
        [OnValueChanged("RefreshPreview")]
        [AssetsOnly]
        public GameObject _targetPrefab;

        [Title("匯出設定")]
        [HideLabel]
        [InlineProperty]
        [OnValueChanged("RefreshPreview")]
        public PrefabExportSettings _settings = new();

        [Title("預覽")]
        [HideLabel]
        [TextArea(20, 50)]
        [ReadOnly]
        public string _preview = string.Empty;

        [PropertySpace(10)]
        [HorizontalGroup("Buttons")]
        [Button("複製到剪貼簿", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 0.4f)]
        private void CopyToClipboard()
        {
            if (_targetPrefab == null)
            {
                EditorUtility.DisplayDialog("錯誤", "請先選擇一個 Prefab", "確定");
                return;
            }

            RefreshPreview();
            EditorGUIUtility.systemCopyBuffer = _preview;
            Debug.Log($"已複製 Prefab \"{_targetPrefab.name}\" 為文字 ({_preview.Length} 字元)");
        }

        [HorizontalGroup("Buttons")]
        [Button("重新整理預覽", ButtonSizes.Large)]
        private void RefreshPreview()
        {
            if (_targetPrefab == null)
            {
                _preview = string.Empty;
                return;
            }

            _settings.InvalidateTypeCache();
            _preview = PrefabToTextExporter.Export(_targetPrefab, _settings);
        }

        [HorizontalGroup("Buttons")]
        [Button("儲存設定", ButtonSizes.Large)]
        private void SaveSettings()
        {
            _settings.Save();
            Debug.Log("匯出設定已儲存");
        }

        [HorizontalGroup("Buttons")]
        [Button("載入設定", ButtonSizes.Large)]
        private void LoadSettings()
        {
            _settings = PrefabExportSettings.Load();
            RefreshPreview();
            Debug.Log("已載入儲存的設定");
        }

        [PropertySpace(20)]
        [Title("工具")]
        [HorizontalGroup("Tools")]
        [Button("清除預設值快取")]
        private void ClearCache()
        {
            PrefabToTextExporter.ClearDefaultCache();
            Debug.Log("已清除預設值快取");
            RefreshPreview();
        }

        [HorizontalGroup("Tools")]
        [Button("重設為預設設定")]
        private void ResetSettings()
        {
            _settings = PrefabExportSettings.CreateDefault();
            RefreshPreview();
        }

        private void OnSelectionChange()
        {
            if (Selection.activeObject == null) return;

            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path.EndsWith(".prefab"))
            {
                _targetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                RefreshPreview();
                Repaint();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            // 載入儲存的設定
            _settings = PrefabExportSettings.Load();

            // 如果有選中的 Prefab，自動載入
            if (Selection.activeObject != null)
            {
                var path = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (path.EndsWith(".prefab"))
                {
                    _targetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    RefreshPreview();
                }
            }
        }
    }
}

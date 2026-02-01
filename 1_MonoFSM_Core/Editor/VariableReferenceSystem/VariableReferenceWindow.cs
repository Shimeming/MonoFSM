using System.Collections.Generic;
using MonoFSM.Variable;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MonoFSM.Editor.VariableReferenceSystem
{
    /// <summary>
    /// Variable Reference Finder - 查找 Variable 被引用的 Editor 視窗
    /// </summary>
    public class VariableReferenceWindow : EditorWindow
    {
        private enum ScanMode
        {
            PrefabStage,
            Scene
        }

        // UI 狀態
        private ScanMode _scanMode = ScanMode.PrefabStage;
        private AbstractMonoVariable _selectedVariable;
        private bool _locked;
        private Vector2 _scrollPos;

        // 搜尋結果
        private List<VariableReferenceInfo> _localReferences = new();
        private List<VariableReferenceInfo> _crossEntityReferences = new();

        // Foldout 狀態
        private bool _localFoldout = true;
        private bool _crossEntityFoldout = true;

        // Scene 模式的根物件
        private GameObject _sceneRoot;

        [MenuItem("Tools/MonoFSM/Variable Reference Finder")]
        public static void ShowWindow()
        {
            GetWindow<VariableReferenceWindow>("Variable Reference Finder");
        }

        /// <summary>
        /// Context Menu: 從 Hierarchy 或 Inspector 右鍵開啟並查找
        /// </summary>
        [MenuItem("CONTEXT/AbstractMonoVariable/Find References")]
        private static void FindReferencesFromContext(MenuCommand command)
        {
            var variable = command.context as AbstractMonoVariable;
            if (variable == null) return;

            ShowWindowWithVariable(variable);
        }

        /// <summary>
        /// 開啟視窗並直接查找指定的 Variable
        /// </summary>
        public static void ShowWindowWithVariable(AbstractMonoVariable variable)
        {
            var window = GetWindow<VariableReferenceWindow>("Variable Reference Finder");

            // 確保有掃描資料
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null)
            {
                VariableReferenceScanner.ScanFromRoot(stage.prefabContentsRoot);
            }
            else if (variable != null)
            {
                // Scene 模式：從 Variable 的根物件開始掃描
                var root = variable.transform.root.gameObject;
                VariableReferenceScanner.ScanFromRoot(root);
                window._sceneRoot = root;
                window._scanMode = ScanMode.Scene;
            }

            window._selectedVariable = variable;
            window._locked = true; // 自動鎖定
            window.UpdateSearchResults();
            window.Repaint();
        }

        private void OnEnable()
        {
            PrefabStage.prefabStageOpened += OnPrefabStageOpened;
            PrefabStage.prefabStageClosing += OnPrefabStageClosing;
            Selection.selectionChanged += OnSelectionChanged;

            // 如果已經在 PrefabStage 中，立即掃描
            TryScanCurrentPrefabStage();
        }

        private void OnDisable()
        {
            PrefabStage.prefabStageOpened -= OnPrefabStageOpened;
            PrefabStage.prefabStageClosing -= OnPrefabStageClosing;
            Selection.selectionChanged -= OnSelectionChanged;
            VariableReferenceScanner.ClearCache();
        }

        private void OnPrefabStageOpened(PrefabStage stage)
        {
            if (_scanMode == ScanMode.PrefabStage)
            {
                VariableReferenceScanner.ScanFromRoot(stage.prefabContentsRoot);
                ClearSelection();
                Repaint();
            }
        }

        private void OnPrefabStageClosing(PrefabStage stage)
        {
            if (_scanMode == ScanMode.PrefabStage)
            {
                VariableReferenceScanner.ClearCache();
                ClearSelection();
                Repaint();
            }
        }

        private void OnSelectionChanged()
        {
            if (_locked) return;

            // 嘗試從選取的物件取得 Variable
            if (Selection.activeGameObject != null)
            {
                var variable = Selection.activeGameObject.GetComponent<AbstractMonoVariable>();
                if (variable != null && variable != _selectedVariable)
                {
                    _selectedVariable = variable;
                    UpdateSearchResults();
                    Repaint();
                }
            }
        }

        private void TryScanCurrentPrefabStage()
        {
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null)
            {
                VariableReferenceScanner.ScanFromRoot(stage.prefabContentsRoot);
            }
        }

        private void ClearSelection()
        {
            _selectedVariable = null;
            _localReferences.Clear();
            _crossEntityReferences.Clear();
        }

        private void UpdateSearchResults()
        {
            _localReferences.Clear();
            _crossEntityReferences.Clear();

            if (_selectedVariable == null) return;

            var allRefs = VariableReferenceScanner.GetReferences(_selectedVariable);

            foreach (var refInfo in allRefs)
            {
                // 跳過自己
                if (refInfo.ReferencingComponent == _selectedVariable) continue;

                if (refInfo.Scope == ReferenceScope.Local)
                    _localReferences.Add(refInfo);
                else
                    _crossEntityReferences.Add(refInfo);
            }
        }

        private void OnGUI()
        {
            DrawHeader();
            DrawModeAndRoot();
            DrawVariableSelector();
            DrawResults();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            EditorGUILayout.LabelField("Variable Reference Finder", EditorStyles.boldLabel);

            GUILayout.FlexibleSpace();

            // Scan 按鈕 (Scene 模式下可用)
            EditorGUI.BeginDisabledGroup(_scanMode == ScanMode.PrefabStage);
            if (GUILayout.Button("Scan", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                if (_sceneRoot != null)
                {
                    VariableReferenceScanner.ScanFromRoot(_sceneRoot);
                    UpdateSearchResults();
                }
            }
            EditorGUI.EndDisabledGroup();

            // 鎖定按鈕
            var lockContent = _locked ? EditorGUIUtility.IconContent("LockIcon-On") : EditorGUIUtility.IconContent("LockIcon");
            if (GUILayout.Button(lockContent, EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                _locked = !_locked;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawModeAndRoot()
        {
            EditorGUILayout.BeginHorizontal();

            // Mode 下拉選單
            EditorGUI.BeginChangeCheck();
            _scanMode = (ScanMode)EditorGUILayout.EnumPopup("Mode", _scanMode, GUILayout.Width(200));
            if (EditorGUI.EndChangeCheck())
            {
                if (_scanMode == ScanMode.PrefabStage)
                {
                    TryScanCurrentPrefabStage();
                }
                else
                {
                    VariableReferenceScanner.ClearCache();
                }
                ClearSelection();
            }

            // 顯示 Root
            if (_scanMode == ScanMode.PrefabStage)
            {
                var stage = PrefabStageUtility.GetCurrentPrefabStage();
                var rootName = stage?.prefabContentsRoot?.name ?? "(No Prefab Open)";
                EditorGUILayout.LabelField($"Root: {rootName}");
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                _sceneRoot = EditorGUILayout.ObjectField("Root", _sceneRoot, typeof(GameObject), true) as GameObject;
                if (EditorGUI.EndChangeCheck() && _sceneRoot != null)
                {
                    VariableReferenceScanner.ScanFromRoot(_sceneRoot);
                    UpdateSearchResults();
                }
            }

            EditorGUILayout.EndHorizontal();

            // 檢查 PrefabStage 是否開啟
            if (_scanMode == ScanMode.PrefabStage)
            {
                var stage = PrefabStageUtility.GetCurrentPrefabStage();
                if (stage == null)
                {
                    EditorGUILayout.HelpBox("Please open a Prefab to use PrefabStage mode.", MessageType.Info);
                }
            }
        }

        private void DrawVariableSelector()
        {
            EditorGUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            _selectedVariable = EditorGUILayout.ObjectField(
                "Select Variable",
                _selectedVariable,
                typeof(AbstractMonoVariable),
                true
            ) as AbstractMonoVariable;

            if (EditorGUI.EndChangeCheck())
            {
                UpdateSearchResults();
            }

            if (_selectedVariable == null)
            {
                EditorGUILayout.HelpBox("Select an AbstractMonoVariable to find its references.", MessageType.Info);
            }
        }

        private void DrawResults()
        {
            if (_selectedVariable == null) return;

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            // Local References
            _localFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_localFoldout, $"Local References ({_localReferences.Count})");
            if (_localFoldout)
            {
                EditorGUI.indentLevel++;
                if (_localReferences.Count == 0)
                {
                    EditorGUILayout.LabelField("No local references found.", EditorStyles.miniLabel);
                }
                else
                {
                    foreach (var refInfo in _localReferences)
                    {
                        DrawReferenceItem(refInfo);
                    }
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(5);

            // Cross-Entity References
            _crossEntityFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_crossEntityFoldout, $"Cross-Entity References ({_crossEntityReferences.Count})");
            if (_crossEntityFoldout)
            {
                EditorGUI.indentLevel++;
                if (_crossEntityReferences.Count == 0)
                {
                    EditorGUILayout.LabelField("No cross-entity references found.", EditorStyles.miniLabel);
                }
                else
                {
                    foreach (var refInfo in _crossEntityReferences)
                    {
                        DrawReferenceItem(refInfo);
                    }
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.EndScrollView();
        }

        private void DrawReferenceItem(VariableReferenceInfo refInfo)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // 第一行：Component 類型 + 引用類型標籤
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(refInfo.ComponentDisplayName, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"[{refInfo.TypeDisplayName}]", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            // 第二行：Field Path
            EditorGUILayout.LabelField($"Field: {refInfo.FieldPath}", EditorStyles.miniLabel);

            // 第三行：Entity (如果是 Cross-Entity)
            if (refInfo.Scope == ReferenceScope.CrossEntity && refInfo.OwnerEntity != null)
            {
                EditorGUILayout.LabelField($"Entity: {refInfo.OwnerEntity.name}", EditorStyles.miniLabel);
            }

            // 按鈕列
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Ping", GUILayout.Width(50)))
            {
                EditorGUIUtility.PingObject(refInfo.ReferencingComponent);
            }

            if (GUILayout.Button("Select", GUILayout.Width(50)))
            {
                Selection.activeObject = refInfo.ReferencingComponent;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
    }
}

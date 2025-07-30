using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MonoFSM.Core.DataProvider;
using MonoFSM.Core.Runtime;
using MonoFSM.Variable;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace MonoFSM.Core.Editor
{
    /// <summary>
    /// 為PropertyOfTypeProvider類別提供簡化的欄位路徑編輯器
    /// 當UseSimplePathEditor為true時，顯示A.B.C風格的編輯介面
    /// </summary>
    [DrawerPriority(2, 0, 0)]
    [CustomEditor(typeof(ValueProvider), true)]
    public class ValueProviderEditorDrawer : BasePathEditorDrawer<ValueProvider>
    {
        // protected override bool CanDrawValueProperty(InspectorProperty property)
        // {
        //     // return false;
        //     // Debug.Log(
        //     //     $"SimpleFieldPathEditorDrawer: Checking if can draw property {property.Name} of type {property.ParentType.Name}");
        //     if (property.ParentType == typeof(ValueRef))
        //         return false;
        //     return true;
        // }
        //
        // public override bool CanDrawTypeFilter(Type type)
        // {
        //     // Debug.Log($"SimpleFieldPathEditorDrawer: Checking if can draw type {type.Name}");
        //     // 排除 ValueRef 及其子類型
        //     return type != typeof(ValueRef);
        //     return !typeof(ValueRef).IsAssignableFrom(type) && base.CanDrawTypeFilter(type);
        // }


        // protected override void DrawPropertyLayout(GUIContent label)
        // {
        //     var target = ValueEntry.SmartValue;
        //     if (target == null)
        //     {
        //         CallNextDrawer(label);
        //         return;
        //     }
        //
        //     // 如果是 ValueRef 類型，讓其他 drawer 處理
        //     if (target is ValueRef)
        //     {
        //         Debug.Log("SimpleFieldPathEditorDrawer: ValueRef detected, delegating to next drawer.");
        //         CallNextDrawer(label);
        //         return;
        //     }
        //
        //     SirenixEditorGUI.BeginBox();
        //
        //     // 繪製UseSimplePathEditor勾選框
        //     var useSimpleEditor = GetUseSimplePathEditor(target);
        //
        //     // EditorGUI.BeginChangeCheck();
        //     // var newUseSimpleEditor = EditorGUILayout.Toggle("使用簡化路徑編輯器 (A.B.C)", useSimpleEditor);
        //     // if (EditorGUI.EndChangeCheck())
        //     // {
        //     //     Undo.RecordObject(target, "切換路徑編輯器模式");
        //     //     SetUseSimplePathEditor(target, newUseSimpleEditor);
        //     //     EditorUtility.SetDirty(target);
        //     // }
        //     //
        //     // EditorGUILayout.Space(3);
        //
        //     if (useSimpleEditor)
        //     {
        //         DrawSimplifiedEditor(target, label);
        //     }
        //     else
        //     {
        //         // 繪製原始的詳細編輯器，但不包含最外層的Box（避免雙重boxing）
        //         SirenixEditorGUI.EndBox();
        //         CallNextDrawer(label);
        //         return;
        //     }
        //
        //     SirenixEditorGUI.EndBox();
        // }
        protected override void DrawTree()
        {
            
            // public override void OnInspectorGUI()
            // {
            // base.OnInspectorGUI();
            var target = (ValueProvider)serializedObject.targetObject;
            if (target == null)
            {
                EditorGUILayout.HelpBox("目標物件為空，無法繪製編輯器。", MessageType.Warning);
                return;
            }

            // 如果是 ValueRef 類型，讓其他 drawer 處理
            // if (target is ValueRef)
            // {
            //     // Debug.Log("SimpleFieldPathEditorDrawer: ValueRef detected, delegating to next drawer.");
            //     // CallNextDrawer();
            //     return;
            // }


            target._declarationName =
                SirenixEditorFields.TextField("宣告名稱", target._declarationName, GUILayout.Height(20));
            SirenixEditorGUI.BeginBox();

            // 繪製UseSimplePathEditor勾選框
            var useSimpleEditor = GetUseSimplePathEditor(target);


            if (useSimpleEditor)
            {
                DrawSimplifiedEditor(target);
            }
            else
            {
                // EditorGUILayout.Space(5);
                //
                // // 繪製原始的詳細編輯器，但不包含最外層的Box（避免雙重boxing）
                // SirenixEditorGUI.EndBox();
                // CallNextDrawer(label);
                base.DrawTree();
                // return;
            }
            
            SirenixEditorGUI.EndBox();
            
        }

        /// <summary>
        /// 繪製entityProvider選擇器
        /// </summary>
        private void DrawEntityProviderSelector(ValueProvider target)
        {
            EditorGUILayout.LabelField("Entity", EditorStyles.boldLabel);

            // 獲取當前的entityProvider
            var currentEntityProvider = target._entityProvider;
            var displayText = currentEntityProvider != null
                ? $"{currentEntityProvider.name} ({currentEntityProvider.GetType().Name})"
                : "-- 選擇實體提供者 --";

            var selectorRect = EditorGUILayout.GetControlRect();
            if (GUI.Button(selectorRect, displayText, EditorStyles.popup))
                ShowEntityProviderSelector(target, currentEntityProvider, selectorRect);


            // 顯示當前entityProvider的詳細資訊
            if (currentEntityProvider != null)
            {
                var style = new GUIStyle(EditorStyles.helpBox)
                {
                    normal = { textColor = new Color(0.2f, 0.6f, 0.2f) }, // 綠色文字
                    fontStyle = FontStyle.Italic
                };
                EditorGUI.BeginDisabledGroup(true);

                SirenixEditorFields.UnityObjectField(currentEntityProvider, typeof(AbstractEntityProvider), true,
                    GUILayout.Height(20));
                EditorGUI.EndDisabledGroup();

                // 顯示實體提供者的描述資訊
                // EditorGUILayout.LabelField($"提供者型別: {currentEntityProvider.GetType().Name}", style);
                if (currentEntityProvider.monoEntity != null)
                    EditorGUILayout.LabelField($"關聯實體: {currentEntityProvider.monoEntity.name}", style);
            }
            else
            {
                // 顯示自動抓取 AbstractEntityProvider 的按鈕
                if (GUILayout.Button("自動抓取 AbstractEntityProvider", GUILayout.Height(25)))
                    AutoGrabEntityProvider(target);
            }
        }

        /// <summary>
        ///     顯示entityProvider選擇器
        /// </summary>
        private void ShowEntityProviderSelector(ValueProvider target, Component currentEntityProvider, Rect rect)
        {
            // 創建entityProvider的選擇器，使用AbstractEntityProvider作為過濾類型
            var selector = new DropDownRefCompSelector(target, typeof(AbstractEntityProvider),
                new DropDownRefAttribute { _findFromParentTransform = false });
            selector.ShowInPopup(rect, 400);

            // 啟用單擊確認選擇
            selector.EnableSingleClickToConfirm();

            selector.SelectionConfirmed += selection =>
            {
                var selectedEntityProvider = selection.FirstOrDefault() as AbstractEntityProvider;
                if (selectedEntityProvider != null || selection.Any()) // 允許選擇null來清空
                {
                    Undo.RecordObject(target, "修改實體提供者");
                    target._entityProvider = selectedEntityProvider;

                    // 當entityProvider改變時，清空相關欄位（因為實體來源變了）
                    // SetVarTag(target, null);
                    // SetPathEntries(target, new List<FieldPathEntry>());

                    // 強制重新繪製，確保UI更新
                    EditorUtility.SetDirty(target);

                    Debug.Log($"EntityProvider changed to: {selectedEntityProvider?.name ?? "null"}");
                }
            };
        }

        /// <summary>
        /// 繪製簡化編輯器（包含entityProvider、varTag和fieldPath）
        /// </summary>
        private void DrawSimplifiedEditor(ValueProvider target)
        {
            // 繪製entityProvider選擇器
            DrawEntityProviderSelector(target);

         

            EditorGUILayout.Space(5);

            // 只有在有varTag欄位時才顯示varTag選擇器
            if (HasVarTagField(target))
            {
                DrawVarTagSelector(target);
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.Space(5);

            // 顯示Root Object資訊
            DrawRootObjectInfo(target);
            // 繪製fieldPath編輯器
            DrawSimplifiedPathEditor(target, target.GetObjectType);
            
            EditorGUILayout.Space(5);
            
            // 顯示最終值型別資訊
            DrawFinalValueTypeInfo(target);
        }

        /// <summary>
        /// 繪製varTag選擇器
        /// </summary>
        private void DrawVarTagSelector(ValueProvider target)
        {
            EditorGUILayout.LabelField("VarTag", EditorStyles.boldLabel);

            // 直接使用 ValueProvider 的 varTag 屬性，確保一致性
            var currentVarTag = target.varTag;
            var displayText = currentVarTag != null ? currentVarTag.name : "-- 選擇變數 --";

            var selectorRect = EditorGUILayout.GetControlRect();
            if (GUI.Button(selectorRect, displayText, EditorStyles.popup))
                ShowVarTagSelector(target, currentVarTag, selectorRect);

            // 顯示當前變數的型別資訊
            if (currentVarTag != null)
            {
                var style = new GUIStyle(EditorStyles.helpBox)
                {
                    normal = { textColor = new Color(0.2f, 0.2f, 0.7f) }, // 藍色文字
                    fontStyle = FontStyle.Italic
                };
                EditorGUI.BeginDisabledGroup(true);
                SirenixEditorFields.UnityObjectField(currentVarTag, typeof(VariableTag), true,
                    GUILayout.Height(20));
                if (target.VarRaw != null)
                    SirenixEditorFields.UnityObjectField(target.VarRaw, typeof(AbstractMonoVariable), true,
                    GUILayout.Height(20));
                EditorGUI.EndDisabledGroup();
                // EditorGUILayout.LabelField($"變數型別: {currentVarTag.ValueType?.Name ?? "未知"}", style);
            }
        }

        /// <summary>
        /// 獲取當前的VariableTag（使用統一的 varTag 屬性）
        /// </summary>
        private VariableTag GetVarTag(PropertyOfTypeProvider target)
        {
            // 如果是 ValueProvider，直接使用其 varTag 屬性
            if (target is ValueProvider valueProvider)
                return valueProvider.varTag;
            return null;
            // 其他類型的 PropertyOfTypeProvider，使用反射
            // var field = target.GetType().GetField("_varTag",
            //     BindingFlags.NonPublic | BindingFlags.Instance);
            // return field?.GetValue(target) as VariableTag;
        }

        /// <summary>
        /// 設定VariableTag（確保與 ValueProvider 同步）
        /// </summary>
        private void SetVarTag(PropertyOfTypeProvider target, VariableTag varTag)
        {
            // 直接設定私有欄位，這會影響到 varTag 屬性
            var field = target.GetType().GetField("_varTag",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(target, varTag);
            
            // 如果是 ValueProvider，確保 DropDownVarTag 屬性也同步
            if (target is ValueProvider valueProvider)
            {
                // DropDownVarTag 的 setter 會設定 _varTag，所以這裡不需要額外處理
                // 因為我們已經直接設定了 _varTag 欄位
            }
        }

        /// <summary>
        /// 顯示varTag選擇器
        /// </summary>
        private void ShowVarTagSelector(ValueProvider target, VariableTag currentVarTag, Rect rect)
        {
            // 創建varTag的選擇器
            var selector = new VarTagPropertySelector(target, currentVarTag);
            selector.ShowInPopup(rect, 400);

            selector.SelectionConfirmed += selection =>
            {
                var selectedVarTag = selection.FirstOrDefault();
                if (selectedVarTag != null || selection.Any()) // 允許選擇null來清空
                {
                    Undo.RecordObject(target, "修改變數標籤");
                    SetVarTag(target, selectedVarTag);

                    // 當變數改變時，清空fieldPath（因為型別可能不同）
                    SetPathEntries(target, new List<FieldPathEntry>());

                    
                 

                    // 強制重新繪製，確保UI更新
                    EditorUtility.SetDirty(target);
                    
                    // 如果是 ValueProvider，觸發相關事件或更新
                    if (target is ValueProvider valueProvider2)
                    {
                        // 強制重新計算 GetObjectType
                        var newObjectType = valueProvider2.GetObjectType;
                        Debug.Log($"VarTag changed to: {selectedVarTag?.name ?? "null"}, new object type: {newObjectType?.Name ?? "null"}");
                    }
                }
                else
                {
                    // 如果選擇「不選擇變數」(null)，確保完整清空相關狀態
                    if (selectedVarTag == null)
                    {
                        // 清空可能的內部快取或狀態
                        if (target is ValueProvider valueProvider)
                            // 強制清空任何內部計算的值
                            try
                            {
                                target.ClearVarTag();
                                // 觸發內部狀態重新計算
                                var _ = valueProvider.GetObjectType;
                                var __ = valueProvider.ValueType;
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning($"清空變數時重新計算型別發生警告: {e.Message}");
                            }

                        Debug.Log("已清空變數選擇，相關欄位已重置");
                    }
                }
            };
        }

        /// <summary>
        /// 顯示最終值型別資訊
        /// </summary>
        private void DrawFinalValueTypeInfo(ValueProvider target)
        {
            EditorGUILayout.LabelField("最終值型別", EditorStyles.boldLabel);

            var valueType = target.ValueType;
            var pathEntries = GetPathEntries(target);
            
            string displayInfo;
            Color textColor;

            try
            {
                if (valueType != null)
                {
                    if (pathEntries.Count > 0)
                    {
                        // 有欄位路徑時，顯示路徑資訊
                        var pathString = BuildPathString(pathEntries);
                        displayInfo = $"路徑結果: {valueType.Name}";
                        if (!string.IsNullOrEmpty(pathString))
                        {
                            displayInfo += $" ({target.GetObjectType.Name + "." + pathString})";
                        }

                        // EditorGUILayout.LabelField("當前路徑: " + target.GetObjectType.Name + "." + currentPath, style);
                        textColor = new Color(0.1f, 0.6f, 0.1f); // 綠色
                    }
                    else
                    {
                        // 沒有欄位路徑時，直接顯示變數型別
                        var varTag = target.varTag;
                        if (varTag != null)
                        {
                            displayInfo = $"變數值: {valueType.Name} (來自變數: {varTag.name})";
                        }
                        else
                        {
                            displayInfo = $"實體值: {valueType.Name}";
                        }
                        textColor = new Color(0.2f, 0.2f, 0.7f); // 藍色
                    }
                }
                else
                {
                    displayInfo = "無法確定型別";
                    textColor = Color.red;
                }
            }
            catch (Exception e)
            {
                displayInfo = $"型別獲取錯誤: {e.Message}";
                textColor = Color.red;
                Debug.LogWarning($"無法獲取ValueType: {e.Message}");
            }

            var style = new GUIStyle(EditorStyles.helpBox)
            {
                normal = { textColor = textColor },
                fontStyle = FontStyle.Bold
            };
            EditorGUILayout.LabelField(displayInfo, style);
        }


        /// <summary>
        /// 檢查目標物件是否有varTag欄位
        /// </summary>
        private bool HasVarTagField(PropertyOfTypeProvider target)
        {
            var field = target.GetType().GetField("_varTag",
                BindingFlags.NonPublic | BindingFlags.Instance);
            return field != null;
        }

        /// <summary>
        /// 顯示GetObjectType資訊
        /// </summary>
        private void DrawRootObjectInfo(ValueProvider target)
        {
            EditorGUILayout.LabelField("起始型別資訊", EditorStyles.boldLabel);

            var objectType = target.GetObjectType;
            var displayInfo = "未知型別";

            // 顯示GetObjectType的型別資訊
            try
            {
                // Debug.Log($"GetObjectType: {objectType?.Name ?? "null"}");
                if (objectType != null)
                {
                    if (HasVarTagField(target))
                    {
                        var varTag = GetVarTag(target);
                        if (varTag != null)
                        {
                            // 有選擇varTag時，顯示變數資訊
                            displayInfo = $"{varTag.name} (型別: {objectType.Name})";
                        }
                        else
                        {
                            // 沒有選擇varTag時，顯示Entity型別
                            var entityName = target.fromEntity;
                            displayInfo = entityName != null
                                ? $"{entityName} (型別: {objectType.Name})"
                                : $"Entity (型別: {objectType.Name})";
                        }
                    }
                    else
                    {
                        // 沒有varTag欄位時，直接顯示GetObjectType
                        displayInfo = $"物件型別: {objectType.Name}";
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"無法獲取GetObjectType資訊: {e.Message}");
                displayInfo = objectType?.Name ?? "獲取失敗";
            }

            var style = new GUIStyle(EditorStyles.helpBox)
            {
                normal = { textColor = new Color(0.1f, 0.5f, 0.8f) }, // 藍色文字
                fontStyle = FontStyle.Bold
            };
            EditorGUILayout.LabelField($"來源: {displayInfo}", style);
        }

        /// <summary>
        ///     自動抓取 AbstractEntityProvider 組件
        /// </summary>
        private void AutoGrabEntityProvider(ValueProvider target)
        {
            try
            {
                var entityProvider = target.GetComponentInParent<AbstractEntityProvider>();

                if (entityProvider != null)
                {
                    Undo.RecordObject(target, "自動設定 EntityProvider");
                    target._entityProvider = entityProvider;
                    EditorUtility.SetDirty(target);

                    Debug.Log($"成功自動設定 EntityProvider: {entityProvider.name} ({entityProvider.GetType().Name})");
                }
                else
                {
                    Debug.LogWarning($"在 {target.name} 及其父子物件中找不到 AbstractEntityProvider 組件");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"自動抓取 EntityProvider 時發生錯誤: {e.Message}");
            }
        }

        /// <summary>
        /// 獲取Entity名稱
        /// </summary>
        // private string GetEntityName(PropertyOfTypeProvider target)
        // {
        //     try
        //     {
        //         //FIXME: 需要這個嗎？
        //         // 嘗試從其他可能的欄位獲取
        //         var entityProviderField = target.GetType().GetField("_monoEntityProvider",
        //             BindingFlags.NonPublic | BindingFlags.Instance);
        //         var entityProvider = entityProviderField?.GetValue(target);
        //         if (entityProvider != null) return entityProvider.GetType().Name;
        //
        //         // 嘗試從ParentEntity獲取名稱
        //         var parentEntityField = target.GetType().GetField("_parentEntity",
        //             BindingFlags.NonPublic | BindingFlags.Instance);
        //
        //         if (parentEntityField?.GetValue(target) is MonoBehaviour parentEntity) return parentEntity.name;
        //
        //         return null;
        //     }
        //     catch
        //     {
        //         return null;
        //     }
        // }
    }
}
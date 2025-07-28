using System.Collections.Generic;
using System.Linq;
using _1_MonoFSM_Core.Runtime.Attributes;
using MonoFSM.Variable;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace MonoFSM.Core.Editor
{
    /// <summary>
    ///     VarTagFilterAttribute 的 OdinAttributeDrawer
    ///     用於過濾 VariableTag 欄位的下拉選項，只顯示符合指定變數類型的 VariableTag
    /// </summary>
    public class VarTagFilterAttributeDrawer : OdinAttributeDrawer<VarTagFilterAttribute>
    {
        protected override bool CanDrawAttributeProperty(InspectorProperty property)
        {
            return property.ValueEntry != null && property.ValueEntry.TypeOfValue == typeof(VariableTag);
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            var currentValue = Property.ValueEntry.WeakSmartValue as VariableTag;

            // 驗證當前選中的 VariableTag 是否符合過濾條件
            if (currentValue != null && Attribute.ExpectedVariableType != null)
            {
                var actualType = currentValue.VariableMonoType;
                var expectedType = Attribute.ExpectedVariableType;
                var isValid = false;

                if (actualType != null)
                {
                    // 完全匹配
                    if (actualType == expectedType)
                        isValid = true;
                    // 檢查是否允許相容類型
                    else if (Attribute.AllowCompatibleTypes && expectedType.IsAssignableFrom(actualType))
                        isValid = true;
                }

                if (!isValid)
                {
                    // 顯示警告訊息
                    var expectedTypeName = expectedType.Name;
                    var actualTypeName = actualType?.Name ?? "Unknown";
                    var warningMessage = !string.IsNullOrEmpty(Attribute.CustomErrorMessage)
                        ? Attribute.CustomErrorMessage
                        : $"選中的 VariableTag 類型不符合期望。期望：{expectedTypeName}，實際：{actualTypeName}";

                    SirenixEditorGUI.WarningMessageBox(warningMessage);
                }
            }

            // 繪製帶過濾功能的 VariableTag 選擇器
            DrawFilteredVariableTagSelector(label, currentValue);
        }

        private void DrawFilteredVariableTagSelector(GUIContent label, VariableTag currentValue)
        {
            // 繪製按鈕來開啟選擇器
            var buttonText = currentValue ? currentValue.name : "None";

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(label);

                if (SirenixEditorGUI.SDFIconButton(buttonText, 16, SdfIconType.CaretDownFill, IconAlignment.RightEdge))
                {
                    var selector = new VarTagFilteredSelector(Attribute);
                    selector.SelectionConfirmed += col =>
                    {
                        Property.ValueEntry.WeakSmartValue = col.FirstOrDefault();
                    };
                    // selector.EnableSingleClickToConfirm();
                    selector.ShowInPopup();
                }
            }
        }
    }

    /// <summary>
    ///     過濾後的 VariableTag 選擇器
    /// </summary>
    public class VarTagFilteredSelector : OdinSelector<VariableTag>
    {
        private readonly VarTagFilterAttribute _filterAttribute;

        public VarTagFilteredSelector(VarTagFilterAttribute filterAttribute)
        {
            _filterAttribute = filterAttribute;
            DrawConfirmSelectionButton = false;
            SelectionTree.Config.SelectMenuItemsOnMouseDown = true;
            SelectionTree.Config.ConfirmSelectionOnDoubleClick = true;
        }

        protected override void BuildSelectionTree(OdinMenuTree tree)
        {
            tree.Config.DrawSearchToolbar = true;

            // 添加 None 選項
            tree.Add("-- None --", null);

            // 獲取過濾後的選項
            var filteredTags = GetFilteredVariableTagOptions();

            if (!filteredTags.Any())
            {
                tree.Add("無符合條件的 VariableTag", null);
                return;
            }

            // 按類型分組
            var groupedTags = filteredTags
                .Where(tag => tag != null)
                .GroupBy(tag => tag.VariableMonoType?.Name ?? "Unknown")
                .OrderBy(g => g.Key)
                .ToList();

            foreach (var group in groupedTags)
            {
                var sortedTags = group.OrderBy(tag => tag.name).ToList();

                foreach (var tag in sortedTags)
                {
                    var displayName = $"{tag.name}";
                    var path = group.Key == "Unknown" ? displayName : $"{group.Key}/{displayName}";

                    tree.Add(path, tag);
                }
            }
        }

        private IEnumerable<VariableTag> GetFilteredVariableTagOptions()
        {
            // 直接搜尋所有 VariableTag assets 並過濾
            return AssetDatabase.FindAssets("t:VariableTag")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => AssetDatabase.LoadAssetAtPath<VariableTag>(path))
                .Where(tag => tag != null && ValidateVariableTag(tag));
        }

        private bool ValidateVariableTag(VariableTag variableTag)
        {
            if (variableTag == null || _filterAttribute.ExpectedVariableType == null)
                return true;

            var actualType = variableTag.VariableMonoType;
            var expectedType = _filterAttribute.ExpectedVariableType;

            if (actualType == null)
                return false;

            // 完全匹配
            if (actualType == expectedType)
                return true;

            // 檢查是否允許相容類型
            if (_filterAttribute.AllowCompatibleTypes)
                return expectedType.IsAssignableFrom(actualType);

            return false;
        }
    }
}
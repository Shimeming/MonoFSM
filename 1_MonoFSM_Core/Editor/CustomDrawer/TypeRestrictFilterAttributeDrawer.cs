using System;
using System.Collections.Generic;
using System.Linq;
using _1_MonoFSM_Core.Runtime.Attributes;
using MonoFSM.Runtime.Mono;
using MonoFSM.Variable;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace MonoFSM.Core.Editor
{
    /// <summary>
    /// TypeRestrictFilterAttribute 的通用 OdinAttributeDrawer
    /// 用於過濾帶有 RestrictType 屬性的欄位下拉選項
    /// </summary>
    public class TypeRestrictFilterAttributeDrawer : OdinAttributeDrawer<TypeRestrictFilterAttribute>
    {
        protected override bool CanDrawAttributeProperty(InspectorProperty property)
        {
            return property.ValueEntry != null &&
                   (property.ValueEntry.TypeOfValue == typeof(VariableTag) ||
                    property.ValueEntry.TypeOfValue == typeof(MonoEntityTag));
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            var currentValue = Property.ValueEntry.WeakSmartValue;

            // 驗證當前選中的值是否符合過濾條件
            if (currentValue != null && Attribute.ExpectedType != null)
            {
                var actualType = Attribute.GetRestrictType(currentValue);
                var expectedType = Attribute.ExpectedType;
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
                        : $"選中的類型不符合期望。期望：{expectedTypeName}，實際：{actualTypeName}";

                    SirenixEditorGUI.WarningMessageBox(warningMessage);
                }
            }

            // 繪製帶過濾功能的選擇器
            DrawFilteredSelector(label, currentValue);
        }

        private void DrawFilteredSelector(GUIContent label, object currentValue)
        {
            // 繪製按鈕來開啟選擇器
            var buttonText = "None";
            if (currentValue is ScriptableObject scriptableObj) buttonText = scriptableObj.name;

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(label);

                if (SirenixEditorGUI.SDFIconButton(buttonText, 16, SdfIconType.CaretDownFill, IconAlignment.RightEdge))
                {
                    var selector = new TypeRestrictFilteredSelector(Attribute, Property.ValueEntry.TypeOfValue);
                    selector.SelectionConfirmed += col =>
                    {
                        Property.ValueEntry.WeakSmartValue = col.FirstOrDefault();
                    };
                    selector.ShowInPopup();
                }
            }
        }
    }

    /// <summary>
    /// 通用的過濾選擇器
    /// </summary>
    public class TypeRestrictFilteredSelector : OdinSelector<ScriptableObject>
    {
        private readonly TypeRestrictFilterAttribute _filterAttribute;
        private readonly Type _targetType;

        public TypeRestrictFilteredSelector(TypeRestrictFilterAttribute filterAttribute, Type targetType)
        {
            _filterAttribute = filterAttribute;
            _targetType = targetType;
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
            var filteredOptions = GetFilteredOptions().ToList();

            if (!filteredOptions.Any())
            {
                tree.Add("無符合條件的選項", null);
                return;
            }

            // 按類型分組
            var groupedOptions = filteredOptions
                .Where(option => option != null)
                .GroupBy(option => GetGroupName(option))
                .OrderBy(g => g.Key)
                .ToList();

            foreach (var group in groupedOptions)
            {
                var sortedOptions = group.OrderBy(option => option.name).ToList();

                foreach (var option in sortedOptions)
                {
                    var displayName = $"{option.name}";
                    var path = group.Key == "Unknown" ? displayName : $"{group.Key}/{displayName}";

                    tree.Add(path, option);
                }
            }
        }

        private IEnumerable<ScriptableObject> GetFilteredOptions()
        {
            // 根據目標類型搜尋對應的 assets 並過濾
            return AssetDatabase.FindAssets(_filterAttribute.GetAssetSearchFilter(_targetType))
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ScriptableObject>)
                .Where(asset => asset != null && ValidateAsset(asset));
        }

        private bool ValidateAsset(ScriptableObject asset)
        {
            if (asset == null || _filterAttribute.ExpectedType == null)
                return true;

            var actualType = _filterAttribute.GetRestrictType(asset);
            var expectedType = _filterAttribute.ExpectedType;

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

        private string GetGroupName(ScriptableObject asset)
        {
            if (asset == null) return "Unknown";

            var restrictType = _filterAttribute.GetRestrictType(asset);
            return restrictType?.Name ?? "Unknown";
        }
    }
}
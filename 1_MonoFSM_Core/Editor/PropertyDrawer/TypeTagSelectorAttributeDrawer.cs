using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.DataProvider;
using MonoFSM.Variable.TypeTag;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace MonoFSM.Core.Editor
{
    /// <summary>
    /// 為標記了 TypeTagSelectorAttribute 的 AbstractTypeTag 欄位提供選擇介面
    /// </summary>
    [DrawerPriority(2)]
    public class TypeTagSelectorAttributeDrawer : OdinAttributeDrawer<TypeTagSelectorAttribute>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var targetObject = Property.ParentValues[0];
            var currentValue = Property.ValueEntry.WeakSmartValue as AbstractTypeTag;
            var fieldType = Property.ValueEntry.TypeOfValue;

            // 顯示標題
            var title = string.IsNullOrEmpty(Attribute.Title)
                ? (label?.text ?? Property.Name)
                : Attribute.Title;

            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

            // 獲取選項
            var options = GetTypeTagOptions(targetObject, fieldType);

            if (options.Count == 0)
            {
                EditorGUILayout.HelpBox(Attribute.NoOptionsMessage, MessageType.Info);
                return;
            }

            // 顯示當前選擇
            var displayText = currentValue != null ? currentValue.name : "-- 選擇 TypeTag --";
            var selectorRect = EditorGUILayout.GetControlRect();

            if (GUI.Button(selectorRect, displayText, EditorStyles.popup))
            {
                ShowTypeTagSelector(options, currentValue, selectorRect);
            }

            // 顯示當前選擇的詳細資訊
            if (currentValue != null)
            {
                var style = new GUIStyle(EditorStyles.helpBox)
                {
                    normal = { textColor = new Color(0.2f, 0.6f, 0.2f) },
                    fontStyle = FontStyle.Italic,
                };

                EditorGUI.BeginDisabledGroup(true);
                SirenixEditorFields.UnityObjectField(
                    currentValue,
                    fieldType,
                    true,
                    GUILayout.Height(20)
                );
                EditorGUI.EndDisabledGroup();

                if (currentValue.Type != null)
                {
                    EditorGUILayout.LabelField($"類型: {currentValue.Type.Name}", style);
                }
            }
        }

        /// <summary>
        /// 獲取 TypeTag 選項
        /// </summary>
        private List<ValueDropdownItem<AbstractTypeTag>> GetTypeTagOptions(
            object targetObject,
            Type fieldType
        )
        {
            if (targetObject == null)
                return new List<ValueDropdownItem<AbstractTypeTag>>();

            // 優先級1: 使用動態 ITypeTagOptionsProvider 介面
            if (Attribute.UseDynamicProvider && targetObject is ITypeTagOptionsProvider provider)
            {
                try
                {
                    var fieldName = Property.Name;
                    // 使用反射調用泛型方法
                    var method = typeof(ITypeTagOptionsProvider)
                        .GetMethod(nameof(ITypeTagOptionsProvider.GetTypeTagOptions))
                        ?.MakeGenericMethod(fieldType);

                    if (method != null)
                    {
                        var result = method.Invoke(provider, new object[] { fieldName });
                        if (
                            result is IEnumerable<ValueDropdownItem<AbstractTypeTag>> dynamicOptions
                        )
                        {
                            var optionsList = dynamicOptions.ToList();
                            if (optionsList.Count > 0)
                                return optionsList;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"透過 ITypeTagOptionsProvider 獲取選項時發生錯誤: {e.Message}");
                }
            }

            // 優先級2: 使用 OptionsProvider 方法
            if (!string.IsNullOrEmpty(Attribute.OptionsProvider))
            {
                var method = targetObject
                    .GetType()
                    .GetMethod(
                        Attribute.OptionsProvider,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                    );

                if (method != null)
                {
                    try
                    {
                        var result = method.Invoke(targetObject, null);
                        if (result is IEnumerable<ValueDropdownItem<AbstractTypeTag>> methodOptions)
                        {
                            return methodOptions.ToList();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(
                            $"調用 {Attribute.OptionsProvider} 方法時發生錯誤: {e.Message}"
                        );
                    }
                }
            }

            // 優先級3: 尋找所有指定型別的 ScriptableObject 資產
            return FindAllTypeTagAssets(fieldType);
        }

        /// <summary>
        /// 尋找所有指定型別的 TypeTag 資產
        /// </summary>
        private List<ValueDropdownItem<AbstractTypeTag>> FindAllTypeTagAssets(Type fieldType)
        {
            var assets = new List<ValueDropdownItem<AbstractTypeTag>>();

            if (!typeof(AbstractTypeTag).IsAssignableFrom(fieldType))
                return assets;

            // 使用 AssetDatabase 尋找所有相關資產
            var guids = AssetDatabase.FindAssets($"t:{fieldType.Name}");

            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath(assetPath, fieldType) as AbstractTypeTag;

                if (asset != null)
                {
                    var displayName = asset.name;
                    if (asset.Type != null)
                    {
                        displayName += $" ({asset.Type.Name})";
                    }

                    assets.Add(new ValueDropdownItem<AbstractTypeTag>(displayName, asset));
                }
            }

            return assets.OrderBy(item => item.Text).ToList();
        }

        /// <summary>
        /// 顯示 TypeTag 選擇器
        /// </summary>
        private void ShowTypeTagSelector(
            List<ValueDropdownItem<AbstractTypeTag>> options,
            AbstractTypeTag currentValue,
            Rect rect
        )
        {
            var menu = new GenericMenu();

            // 添加「清空選擇」選項
            if (Attribute.ShowClearOption)
            {
                menu.AddItem(
                    new GUIContent("-- 清空選擇 --"),
                    currentValue == null,
                    () =>
                    {
                        Property.ValueEntry.WeakSmartValue = null;
                        Property.MarkSerializationRootDirty();
                    }
                );

                if (options.Count > 0)
                    menu.AddSeparator("");
            }

            // 添加可用的選項
            foreach (var option in options)
            {
                var typeTag = option.Value;
                var isSelected = currentValue == typeTag;

                menu.AddItem(
                    new GUIContent(option.Text),
                    isSelected,
                    () =>
                    {
                        Property.ValueEntry.WeakSmartValue = typeTag;
                        Property.MarkSerializationRootDirty();
                    }
                );
            }

            menu.ShowAsContext();
        }
    }
}

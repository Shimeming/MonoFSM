using System;
using System.Linq;
using System.Reflection;
using MonoFSM.Core;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.Editor.Utility;
using MonoFSM.Foundation;
using MonoFSM.Runtime;
using MonoFSM.Variable;
using MonoFSM.Variable.SOType;
using MonoFSM.Variable.TypeTag;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

// using MonoFSM.Core.Editor.Utility;

namespace _1_MonoFSM_Core.Runtime._1_States
{
    //要有drawer去自己mapping?
    // public class VarTagMappingAttribute : PropertyAttribute
    // {
    //     // This attribute can be used to filter variables by tag.
    //     // It can be used to prevent mapping of certain variables in the schema.
    //     public string TagName { get; }
    //
    //     public VarTagMappingAttribute(string tagName)
    //     {
    //         TagName = tagName;
    //     }
    // }


    //FIXME: 和MonoEntity有沒有定位重疊？
    // public class VarTagAttribute
    //一個entity應給要允許多個schema, 或是說多個variable folder?
    /// <summary>
    ///     定義 data structure 就好，不要寫邏輯, ECS?
    /// </summary>
    [Searchable]
    public abstract class AbstractEntitySchema
        : AbstractDescriptionBehaviour,
            IStringKey,
            IValueOfKey<AbstractEntitySchema>
    {
        protected override string DescriptionTag => "Schema";

        // This class can be used to define a schema for MonoEntity,
        // which can be used to map variables and components automatically.
        // It can also be used to define a schema for a specific entity type.
        [Required]
        [PreviewInInspector]
        [AutoParent]
        private MonoEntity _parentEntity;
        public MonoEntity BindEntity => _parentEntity;

        [SOConfig("TypeTags")]
        [InfoBox("Schema 對應的類型標籤，用於識別此 Schema 的類型")]
        public MonoTypeTag _typeTag;

        [Button]
        public void Mapping()
        {
            if (_parentEntity == null)
            {
                Debug.LogError("Parent Entity 為空，無法執行 Mapping", this);
                return;
            }

            if (_parentEntity.VariableFolder == null)
            {
                Debug.LogError("VariableFolder 為空，無法執行 Mapping", this);
                return;
            }

            // 0. 首先確保有 TypeTag
            AutoAssignTypeTag();

            // 1. Get All VarWrapper fields
            var varWrapperFields = GetVarWrapperFields();
            Debug.Log($"找到 {varWrapperFields.Length} 個 VarWrapper 欄位", this);
            foreach (var field in varWrapperFields)
                ProcessVarWrapperField(field);

            // 向後相容：保留舊的 VarTagMappingAttribute 支援
            // var legacyFields = GetFieldsWithVarTagMappingAttribute();
            // foreach (var field in legacyFields) ProcessLegacyField(field);
        }

        [Button]
        [InfoBox("自動為此 Schema 指派對應的 TypeTag")]
        public void AutoAssignTypeTag()
        {
            if (_typeTag != null)
                // Debug.Log($"已有 TypeTag: {_typeTag.name}", this);
                return;

            var manager = SchemaTypeTagManager.Instance;
            _typeTag = manager.GetOrCreateTypeTagForSchema(GetType());

            if (_typeTag != null)
            {
                Debug.Log($"自動指派 TypeTag: {_typeTag.name}", this);
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
            else
            {
                Debug.LogWarning($"無法為 {GetType().Name} 獲取或創建 TypeTag", this);
            }
        }

        private FieldInfo[] GetVarWrapperFields()
        {
            return GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(field => IsVarWrapperType(field.FieldType))
                .ToArray();
        }

        private bool IsVarWrapperType(Type type)
        {
            Debug.Log($"檢查類型 {type.Name} 是否為 VarWrapper<,> 的實例", this);
            // 檢查是否是 VarWrapper<,> 的泛型實例
            // if (!type.IsGenericType) return false;
            if (type.InheritsFrom(typeof(VarWrapper<,>)))
                // 這裡可以進一步檢查 VarWrapper 的具體類型
                return true;
            return false;
        }

        private void ProcessVarWrapperField(FieldInfo field)
        {
            var wrapperInstance = field.GetValue(this);
            if (wrapperInstance == null)
            {
                Debug.LogWarning($"VarWrapper 欄位 {field.Name} 為 null", this);
                return;
            }

            // 優先檢查是否已經有設定 _bindTag
            var bindTagField = wrapperInstance.GetType().GetField("_bindTag");
            var existingTag = bindTagField?.GetValue(wrapperInstance) as VariableTag;

            VariableTag targetTag;

            if (existingTag != null)
            {
                // 如果已經有 _bindTag，直接使用
                targetTag = existingTag;
                Debug.Log($"VarWrapper 欄位 {field.Name} 已有 _bindTag: {existingTag.name}", this);
            }
            else
            {
                // 如果沒有 _bindTag，才使用字串比對搜尋
                var fieldName = field.Name.TrimStart('_');
                Debug.Log(
                    $"VarWrapper 欄位 {field.Name} 沒有 _bindTag，搜尋 VariableTag: {fieldName}",
                    this
                );

#if UNITY_EDITOR
                var foundTags = SOUtility.FindAssetsByName<VariableTag>(fieldName);
                // targetTag = SOUtility.FindAssetByExactName<VariableTag>(fieldName);
                if (foundTags != null && foundTags.Count() > 0)
                {
                    targetTag = foundTags.First();
                    Debug.Log($"找到匹配的 VariableTag: {targetTag.name}，設定到 _bindTag", this);
                    // 設定 VarWrapper 的 _bindTag 欄位
                    SetVarWrapperBindTag(field, targetTag);
                }
                else
                {
                    Debug.LogWarning($"找不到名稱為 '{fieldName}' 的 VariableTag", this);
                    return;
                }
#endif
            }

            // 使用 targetTag 尋找並設定對應的變數
            if (targetTag != null)
                SetVarWrapperVariable(field, targetTag);
        }

        private void SetVarWrapperBindTag(FieldInfo wrapperField, VariableTag variableTag)
        {
            var wrapperInstance = wrapperField.GetValue(this);
            if (wrapperInstance == null)
            {
                // 如果 wrapper 實例為 null，創建一個新的實例
                wrapperInstance = Activator.CreateInstance(wrapperField.FieldType);
                wrapperField.SetValue(this, wrapperInstance);
            }

            // 設定 _bindTag 欄位
            var bindTagField = wrapperField.FieldType.GetField("_bindTag");
            if (bindTagField != null)
            {
                bindTagField.SetValue(wrapperInstance, variableTag);
                Debug.Log($"已設定 {wrapperField.Name}._bindTag = {variableTag.name}", this);
            }
        }

        private void SetVarWrapperVariable(FieldInfo wrapperField, VariableTag variableTag)
        {
            // 根據 tag 尋找對應的變數
            var existingVar = _parentEntity.VariableFolder.GetVariable(variableTag);

            var wrapperInstance = wrapperField.GetValue(this);
            var varField = wrapperField.FieldType.GetField("_var");

            if (varField != null)
            {
                if (existingVar != null)
                {
                    // 變數存在，直接指派
                    if (varField.FieldType.IsAssignableFrom(existingVar.GetType()))
                    {
                        varField.SetValue(wrapperInstance, existingVar);
                        Debug.Log($"已將現有變數指派到 {wrapperField.Name}._var", this);
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"變數類型不匹配: 期望 {varField.FieldType}, 實際 {existingVar.GetType()}",
                            this
                        );
                    }
                }
                else
                {
                    // 變數不存在，創建新的變數
                    var newVar = _parentEntity.VariableFolder.CreateVariableWithTag(
                        varField.FieldType,
                        variableTag
                    );
                    if (newVar != null)
                    {
                        varField.SetValue(wrapperInstance, newVar);
                        Debug.Log($"已創建並指派新變數到 {wrapperField.Name}._var", this);
                    }
                }
            }
        }

        private AbstractMonoVariable CreateVariableForField(FieldInfo field, string tagName)
        {
            // 使用 VariableFolder 的 CreateVariable 方法來創建變數
            return _parentEntity.VariableFolder.CreateVariable(field.FieldType, tagName);
        }

        public string GetStringKey => GetType().Name;

        public AbstractEntitySchema Key => this;
    }
}

using System;
using System.Linq;
using System.Text.RegularExpressions;
using _1_MonoFSM_Core.Runtime.Attributes;
using MonoFSM.Core.Attributes;
using MonoFSM.Variable.FieldReference;
using MonoFSM.Variable.TypeTag;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSM.Variable
{
    public interface IVariableTagSetter
    {
        VariableTag refVariableTag { get; }
    }

    public interface IStringKey
    {
        public string GetStringKey { get; }
    }

    [CreateAssetMenu(menuName = "Assets/MonoFSM/VariableTag")]
    public class VariableTag : ScriptableObject, IStringKey, IProxyType //, IFloatValue , SceneSave?
    {
        private void OnValidate()
        {
            _variableType._bindObject = this;
            _valueFilterType._bindObject = this;
        }

        /// <summary>
        /// 變數綁定的型別，通常是 MonoVariable 或其子類別
        /// </summary>
        [ShowInInspector]
        [DisplayAsString]
        [PropertyOrder(-1)]
        [LabelText("變數綁定型別")]
        public Type VariableMonoType => _variableTypeTag?.Type ?? _variableType.RestrictType;

        // [ShowDrawerChain]
        [SOConfig("VarMonoTypeTags")]
        [TypeRestrictDropdown(typeof(VarMonoTypeTag))]
        // [FormerlySerializedAs("_variableTypeData")]
        public VarMonoTypeTag _variableTypeTag;

        [SOConfig("objectValueTypeTags")]
        // [FormerlySerializedAs("_valueTypeData")]
        public ValueTypeTag _valueTypeTag;

        //SystemTypeData

        [ShowInInspector]
        [DisplayAsString]
        [PropertyOrder(-1)]
        [LabelText("變數數值型別")]
        public Type ValueType => _valueTypeTag?.Type ?? _valueFilterType.RestrictType;

        //FIXME: 限定型別？
        //FIXME: 下拉式巢狀分類:
        // sampleData? sampleDescriptableTag?
        GameFlagBase SampleData;

        [Button]
        public void SyncValueFilterTypeWithVariableType()
        {
            var variableType = _variableTypeTag?.Type ?? _variableType?.RestrictType;
            if (variableType == null)
                return;

            Type tValueType = null;
            var currentType = variableType;
            while (currentType != null && currentType != typeof(object))
            {
                if (currentType.IsGenericType)
                {
                    var genericTypeDef = currentType.GetGenericTypeDefinition();
                    if (genericTypeDef == typeof(AbstractFieldVariable<,,>))
                    {
                        tValueType = currentType.GetGenericArguments()[2];
                        break;
                    }

                    if (genericTypeDef == typeof(GenericUnityObjectVariable<>))
                    {
                        tValueType = currentType.GetGenericArguments()[0];
                        break;
                    }
                }

                currentType = currentType.BaseType;
            }

            if (tValueType != null)
                _valueFilterType.SetBaseType(tValueType);
        }

        [Button]
        void RefreshStringKey()
        {
            _cachedStringKey = null;
            var result = GetStringKey;
        }

        //scriptable object會殘留？
        [NonSerialized]
        string _cachedStringKey;

        [PreviewInInspector]
        public string GetStringKey
        {
            get
            {
                //remove Characters between '[' and ']'

                _cachedStringKey = Regex.Replace(name, @"\[.*?\]", string.Empty);
                _cachedStringKey = Regex.Replace(_cachedStringKey, @"\s+", string.Empty);
                // _cachedStringKey = name.Replace(" ", "");
                return _cachedStringKey;
            }
        }

#if UNITY_EDITOR
        [HideInInlineEditors]
        [TextArea]
        public string Note;
#endif

        //可以DI標記variable類型，像是血量？要降低對方的血量之類的
        // [InlineProperty]
        [Obsolete("use _variableTypeTag")]
        [HideInInlineEditors]
        public MySerializedType<AbstractMonoVariable> _variableType; //我這個variable是什麼型別

        [Obsolete] //名字不好.._valueRestrictType?
        public MySerializedType<object> _valueFilterType; //自動化的部分要改成去動tag? 但好像不該動tag?

        public Type ValueFilterType => _valueTypeTag?.Type ?? _valueFilterType.RestrictType;

        [Button]
        void FetchFilterType()
        {
            //FIXME: 好像拿不到...
        }

        //FIXME: Editor time 把雙向連結撈出來
#if UNITY_EDITOR

        [PreviewInInspector]
        AbstractMonoVariable[] bindedVariables;

        // [OnInspectorGUI] //會lag?
        [Button]
        void GetBindedVariables()
        {
            bindedVariables = FindObjectsByType<AbstractMonoVariable>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                )
                .Where(v => v._varTag == this)
                .ToArray();
            bindedVariableSetters = FindObjectsByType<MonoBehaviour>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                )
                .OfType<IVariableTagSetter>()
                .Where(v => v.refVariableTag == this)
                .ToArray();
        }

        [PreviewInInspector]
        IVariableTagSetter[] bindedVariableSetters;

        /// <summary>
        /// 🆕 同步此 VariableTag 中的所有型別引用
        /// </summary>
        [Button("同步所有型別引用")]
        public void SyncAllTypeReferences()
        {
            RefactorSafeHelper.SyncVariableTagTypes(this);
        }

        /// <summary>
        /// 🆕 檢查此 VariableTag 中的所有型別同步狀態
        /// </summary>
        [Button("檢查型別同步狀態")]
        public void CheckAllTypeReferencesSync()
        {
            RefactorSafeHelper.CheckVariableTagTypesSync(this);
        }
#endif

        public Type GetProxyType()
        {
            return VariableMonoType;
        }
    }
}

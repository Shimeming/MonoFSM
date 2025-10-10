using System;
using MonoFSM.Core.Attributes;
using MonoFSM.Variable.TypeTag;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace MonoFSM.Variable
{
    // [InlineField]
    [Serializable]
    public class VarCompField<T>
        where T : class //用attribute processor幫她加inlinefield
    {
        public bool HasVar => _varComp != null;
        [SerializeField]
        VarComp _varComp;
        public T Value => _varComp.Value as T;
    }

    //variable
    //Monobehaviour 包著一個變數
    //需要Generic嗎...好像算了
    //沒用的東西！
    //FIXME: 怎麼表達Required / Optional 的variable?
    public class VarComp : GenericUnityObjectVariable<Component>
    {
        //FIXME: typeRestrict

        [Header("類型限制")]
        [SerializeField]
        [Tooltip("限定 Component 的類型")]
        [SOConfig("TypeTag")]
        private CompTypeTag _componentTypeTag;

        //FIXME: isConst時要Required? 怎麼在 AbstractDescriptionBehaviour 檢查？
        [HideIf(nameof(HasParentVarEntity))]
        [Header("預設值")]
        [ShowInInspector]
        [DropDownRef(null, nameof(SiblingValueFilter))]
        private Component SiblingDefaultValue
        {
            set => _defaultValue = value;
            get => _defaultValue;
        } //用property?

        //可以篩選type, 就是宣告
        Type SiblingValueFilter()
        {
            // 優先使用 MonoTypeTag 的類型限制
            if (_componentTypeTag != null && _componentTypeTag.Type != null)
                return _componentTypeTag.Type;

            // 退回到使用 _varTag 的類型限制
            if (_varTag == null)
                return typeof(Component);
            // Debug.Log("RestrictType is " + _varTag.ValueFilterType, _varTag);
            return _varTag.ValueFilterType;
        }

        //FIXME: 繼承時想要加更多attribute
        // [Header("預設值")] [HideIf(nameof(_siblingDefaultValue))] [SerializeField]
        // protected Component _defaultValue;


        // protected Component DefaultValue =>
        //     _siblingDefaultValue != null ? _siblingDefaultValue : _defaultValue;
        //FIXME: 把Variable直接丟到該模組上就好？
        // IEnumerable<Component> _filter()
        // {
        //     // var type = serializedType.Value;
        //     // if(type == null)
        //     //     return null;
        //     // return this.GetComponentsOfSibling<LevelRunner>(type);
        // }

        // [ValueDropdown(nameof(_filter))]
        // [SerializeField]
        // private Component DefaultValue;

        // [TypeDrawerSettings(BaseType = typeof(Component)), ShowInInspector]
        // public Type type; //FIXME: 要用string 回推 type?
        // public override GameFlagBase FinalData => null;
        // public override void ResetToDefaultValue()
        // {
        //     _currentValue = DefaultValue;
        // }
    }
}

using System;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.DataProvider;
using MonoFSM.EditorExtension;
using MonoFSM.Variable.Attributes;
using MonoFSMCore.Runtime.LifeCycle;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MonoFSM.Variable
{
    [Searchable]
    public abstract class AbstractObjectVariable : AbstractMonoVariable
    {
        //FIXME: 這個是多的嗎？
        [PreviewInDebugMode]
        public abstract Object RawValue { get; } //不一定有Object可以returnㄅ？
        public abstract void ClearValue();
        // protected void SetValueExecution();
    }

    public abstract class GenericUnityObjectVariable<TValueType>
        : AbstractObjectVariable,
            ISettable<TValueType>,
            IResetStateRestore,
            IHierarchyValueInfo
        where TValueType : Object
    {
        [CompRef]
        [AutoChildren(DepthOneOnly = true)]
        private IValueProvider<TValueType>[] _valueSources; //FIXME: 可能會有多個耶...還要一層resolver嗎？

        private bool IsUsingValueSource => _valueSources is { Length: > 0 }; //要cached bool? 這塊絕對是config而已

        [ShowInPlayMode]
        private IValueProvider<TValueType> valueSource
        {
            get
            {
                if (!IsUsingValueSource)
                    return null;
                foreach (var valueProvider in _valueSources)
                    if (valueProvider.IsValid)
                        return valueProvider;

                //[]: 多個的話要怎麼辦？還是說不允許多個？
                Debug.LogWarning(
                    "condition not met, use default? (last)" + _valueSources[^1],
                    this
                );
                return _valueSources[^1];
            }
        }

        public override bool IsValueExist => _currentValue != null;

        //FIXME: 繼承時想要加更多attribute
        // [Header("預設值")] [HideIf(nameof(_siblingDefaultValue))]
        [HideIf(nameof(IsUsingValueSource))]
        [SOConfig("10_Flags/GameData", useVarTagRestrictType: true)] //痾，只有SO類才需要ㄅ
        [SerializeField]
        [Required]
        protected TValueType _defaultValue;

        protected virtual TValueType DefaultValue
        {
            get { return _defaultValue; }
            // set { _defaultValue = value; }
        }

        // _siblingDefaultValue != null ? _siblingDefaultValue : _defaultValue;

        /// <summary>
        /// 保持原有的強型別Value屬性，但使用動態轉型
        /// </summary>
        [PreviewInDebugMode]
        [DynamicType] // 標示此屬性的型別會根據VarTag動態決定
        public TValueType Value //動態？ varTag的RestrictType 才決定型別？
        {
            get
            {
                if (IsUsingValueSource)
                    return valueSource.Value;

                if (!Application.isPlaying)
                    return DefaultValue;
                return _currentValue;
            }
        }

        public override Object RawValue => Value; //FIXME: 用Object?

        // public T Value => _currentValue;
        //green
        [GUIColor(0.2f, 0.8f, 0.2f)]
        [PreviewInInspector]
        // [InlineEditor]
        protected TValueType _currentValue; //要用ObjectField? 這樣才統一？ Object不可能做成GameFlag/Data?

        //所有人都不該set這個

        [PreviewInDebugMode]
        private TValueType _lastValue;

        [PreviewInDebugMode]
        private TValueType _lastNonNullValue;

        public void CommitValue()
        {
            _lastValue = _currentValue;
            if (_currentValue != null)
                _lastNonNullValue = _currentValue;
        }

        public void SetValue(object value, MonoBehaviour byWho)
        {
            SetValue<TValueType>((TValueType)value, byWho);
        }

        public void SetValue(TValueType value, MonoBehaviour byWho)
        {
            SetValue<TValueType>(value, byWho);
        }

        //怎麼那麼多種...
        protected override void SetValueInternal<T1>(T1 value, Object byWho)
        {
            //沒有實作唷
            //沒有ObjectField...
            // Debug.Log("Set value to " + value, this);
            //FIXME: 這需要分開嗎？在寫啥

            _currentValue = value as TValueType;
            RecordSetbyWho(_currentValue, byWho as MonoBehaviour);
            // OnValueChanged?.Invoke(_currentValue); //多一個參數的版本
            OnValueChanged();

#if UNITY_EDITOR
            _lastSetByWho = byWho;
#endif
        }

#if UNITY_EDITOR
        [PreviewInDebugMode]
        private Object _lastSetByWho;
#endif

        public override void ClearValue()
        {
            SetValueInternal<Object>(null, this);
            // _currentValue = null;
        }

        // public override GameFlagBase FinalData { get; }
        // public override Type FinalDataType => RawValue != null ? RawValue.GetType() : null; //指的是DescriptableData
        public override Type ValueType => typeof(TValueType);

        //FIXME: 好亂喔QQ
        public override object objectValue => Value;

        // public override Component objectValue => RawValue;


        //FIXME: Editor用的...EditorObjectValue?
        //         public Object EditorValue
        //         {
        //             get => DefaultValue;
        //             set
        //             {
        //                 _defaultValue = value as TValueType;
        // #if UNITY_EDITOR
        //                 EditorUtility.SetDirty(this);
        // #endif
        //             }
        //         }

        // public Type ObjectType => typeof(TValueType);

        public override void ResetStateRestore()
        {
            //這裡才做會不會太晚？
            if (_isConst) //FIXME: 怪怪的, 但現在 playerTransform有在用, set過後不想被reset, 可能要另外處理這個情境？
                return;
            SetValueInternal(DefaultValue, this);
        }

        [PropertyOrder(-1)]
        [Header("避免關卡重置時清除資料")]
        [SerializeField]
        public bool _isConst; //

        //避免reset restore?
        public string ValueInfo => Value != null ? Value.name : "null";
        public bool IsDrawingValueInfo => true;
    }
}

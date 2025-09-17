using System;
using System.Collections.Generic;
using System.Threading;
using _1_MonoFSM_Core.Runtime.Attributes;
using MonoFSM.Core.Attributes;
using MonoFSM.EditorExtension;
using MonoFSMCore.Runtime.LifeCycle;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Search;
using Object = UnityEngine.Object;

namespace MonoFSM.Variable
{
    [Searchable]
    // public abstract class AbstractObjectVariable : AbstractMonoVariable
    // {
    //     //FIXME: 這個是多的嗎？
    //     [PreviewInDebugMode]
    //     public abstract Object RawValue { get; } //不一定有Object可以returnㄅ？
    //
    //     // protected void SetValueExecution();
    // }
    public abstract class GenericUnityObjectVariable<TValueType>
        : TypedMonoVariable<TValueType>,
            ISettable<TValueType>,
            IResetStateRestore,
            IHierarchyValueInfo
        where TValueType : Object
    {
        // 遞迴檢查相關的靜態成員
        private static readonly ThreadLocal<int> _recursionDepth = new(() => 0);

        private static readonly ThreadLocal<
            HashSet<GenericUnityObjectVariable<TValueType>>
        > _visitedVariables = new(() => new HashSet<GenericUnityObjectVariable<TValueType>>());

        private const int MAX_RECURSION_DEPTH = 10;

        public override bool IsValueExist => _currentValue != null;

        //FIXME: 繼承時想要加更多attribute
        // [Header("預設值")] [HideIf(nameof(_siblingDefaultValue))]

        [SerializeField]
        protected TValueType _defaultValue; //ConfigSettingValue? //只有VarMonoObj才需要？

        // _siblingDefaultValue != null ? _siblingDefaultValue : _defaultValue;

        /// <summary>
        /// 保持原有的強型別Value屬性，但使用動態轉型
        /// </summary>
        [GUIColor(0.2f, 0.8f, 0.2f)]
        [PreviewInInspector]
        [DynamicType] // 標示此屬性的型別會根據VarTag動態決定 FIXME: 在做啥？
        public TValueType Value //動態？ varTag的RestrictType 才決定型別？
        {
            get
            {
                // Debug.Log($"Accessing Value of {name}", this);
                // 檢查遞迴深度
                if (_recursionDepth.Value >= MAX_RECURSION_DEPTH)
                {
                    Debug.LogError(
                        $"[Stack Overflow Protection] Maximum recursion depth ({MAX_RECURSION_DEPTH}) reached in variable: {name}",
                        this
                    );
                    Debug.Break();
                    return null;
                }

                // 檢查循環引用
                if (_visitedVariables.Value.Contains(this))
                {
                    Debug.LogError(
                        $"[Circular Reference Protection] Circular reference detected in variable: {name}",
                        this
                    );
                    Debug.Break();
                    return null;
                }

                // 進入遞迴保護區域
                _recursionDepth.Value++;
                _visitedVariables.Value.Add(this);

                try
                {
                    return GetValueInternal();
                }
                finally
                {
                    // 確保清理狀態，即使發生異常也要執行
                    _recursionDepth.Value--;
                    _visitedVariables.Value.Remove(this);

                    // 如果回到最頂層，清理 visited set
                    if (_recursionDepth.Value == 0)
                        _visitedVariables.Value.Clear();
                }
            }
        }

        [ShowInInspector]
        private string _valueDebugStatus;

        /// <summary>
        ///     實際的 Value 獲取邏輯，從原本的 Value getter 分離出來
        /// </summary>
        private TValueType GetValueInternal()
        {
            // Debug.Log($"Getting value of {name}", this);
            //要擋掉嗎？那Editor Time就都不要顯示？
            // if (!Application.isPlaying)
            //     return DefaultValue;
            if (HasParentVarEntity)
            {
                _valueDebugStatus = "Resolving from ParentVarEntity";
                if (_parentVarEntity.Value == null)
                {
                    // Debug.LogError(
                    //     $"{name}'s ParentVarEntity is null, cannot resolve var '{_varTag}'",
                    //     this
                    // );
                    //editor time 沒有正常
                    _valueDebugStatus = "ParentVarEntity is null";
                    return null; //還沒有entity, 過掉？
                }

                // 檢查自我引用（保留原有檢查）
                if (_parentVarEntity == this || _parentVarEntity.Value.GetVar(_varTag) == this)
                {
                    _valueDebugStatus = "Self reference detected";
                    Debug.LogError("ParentVarEntity cannot be self", this);
                    Debug.Break();
                    return null;
                }
                else
                {
                    var targetVar = _parentVarEntity.Value.GetVar(_varTag);
                    if (targetVar == null)
                    {
                        _valueDebugStatus = "Target variable not found in ParentVarEntity";
                        Debug.LogError(
                            $"{name}'s ParentVarEntity has no var: '{_varTag}' folder:{_parentVarEntity.Value.VariableFolder}",
                            _parentVarEntity.Value
                        );
                        Debug.Break();
                        return null;
                    }

                    // 額外的循環引用檢查
                    if (targetVar is GenericUnityObjectVariable<TValueType> targetGenericVar)
                        if (_visitedVariables.Value.Contains(targetGenericVar))
                        {
                            Debug.LogError(
                                $"[Circular Reference Protection] Detected circular reference through ParentVarEntity chain: {name} -> {targetVar.name}",
                                this
                            );
                            Debug.Break();
                            return null;
                        }

                    _valueDebugStatus = $"Resolved from ParentVarEntity: {targetVar.name}";
                    return targetVar.GetValue<TValueType>();
                }
            }

            if (HasValueProvider) //FIXME: 和field 分開寫很鳥?
            {
                _valueDebugStatus = "Resolving from ValueProvider";
                if (Application.isPlaying)
                {
                    if (valueSource == null) //有可能resolve後是null
                        return null;

                    return valueSource.Get<TValueType>();
                }
            }

            if (Application.isPlaying == false)
            {
                _valueDebugStatus = "Using _DefaultValue (Edit Mode)";
                return _defaultValue;
            }

            return _currentValue;
        }

        // public override Object RawValue => Value; //FIXME: 用Object?

        // public T Value => _currentValue;
        //green
        //FIXME: 不該？
        // [InlineEditor]
        [ShowInDebugMode]
        protected TValueType _currentValue; //要用ObjectField? 這樣才統一？ Object不可能做成GameFlag/Data?

        //所有人都不該set這個

        [PreviewInDebugMode]
        private TValueType _lastValue;

        [PreviewInDebugMode]
        private TValueType _lastNonNullValue;

        public override void CommitValue()
        {
            _lastValue = _currentValue;
            if (_currentValue != null)
                _lastNonNullValue = _currentValue;
        }

        //FIXME: 不該留這個API
        // public override void SetValue(object value, MonoBehaviour byWho) //這好蠢？
        // {
        //     //這個trace好討厭...又跑下去，然後再上來internal
        //     SetValue<TValueType>((TValueType)value, byWho);
        // }



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
            SetValueInternal(_defaultValue, this);
        }

        [PropertyOrder(-1)]
        [Header("避免關卡重置時清除資料")]
        [SerializeField]
        public bool _isConst; //

        //避免reset restore?
        public virtual string ValueInfo
        {
            get
            {
                // ValueInfo 也需要保護，因為它會呼叫 Value
                try
                {
                    var value = Value;
                    return value != null ? value.name : "null";
                }
                catch (Exception ex)
                    when (ex.Message.Contains("recursion") || ex.Message.Contains("circular"))
                {
                    return "[Recursion Error]";
                }
            }
        }

        public bool IsDrawingValueInfo => true;
    }
}

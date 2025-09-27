using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.Utilities;
using MonoFSM.Runtime;
using MonoFSM.Runtime.Mono;
using MonoFSM.Runtime.Variable;
using MonoFSM.Variable;
using MonoFSM.Variable.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace MonoFSM.Core.DataProvider
{
    public enum GetFromType
    {
        ParentVarOwner,
        GlobalInstance,
        VariableOwnerProvider,
    }

    //負責提供一個MonoVar，需要一個BlackboardProvider

    //TODO: FIXME: drag drop reference後，自動填入tag/monoTag
    //隱含有Parent VariableOwner的概念是不是不太好？
    //動態提取？不用type? 讓我pathField選到string就有string的能力？ any IValueProvider然後Get<TType>看看？
    //A.B.Variable, 用tag來找variable
    //單純版好像不想要這麼複雜？ localRef
    [Obsolete]
    public abstract class VariableProviderRef<TVarMonoType, TValueType>
        : AbstractVariableProviderRef,
            IVariableProvider,
            IValueProvider<TValueType> //, IStringProvider,IValueProvider<TValueType>
        where TVarMonoType : AbstractMonoVariable
    {
        public override object StartingObject => VarRaw;

        public override Type GetObjectType
        {
            get
            {
                // 對於支援動態型別的變數，返回其ValueType而非固定的變數型別
                if (VarRaw != null && HasDynamicValueType(VarRaw))
                {
                    return VarRaw.ValueType;
                }

                return typeof(TVarMonoType);
            }
        }

        /// <summary>
        /// 檢查變數是否支援動態型別（目前僅VarGameData）
        /// </summary>
        private bool HasDynamicValueType(AbstractMonoVariable variable)
        {
            // 檢查是否為VarGameData或其他支援動態型別的變數
            return variable.GetType().Name.Contains("VarGameData")
                && variable._varTag?.ValueFilterType != null;
        }

        [OnValueChanged(nameof(OnVarOwnerChange))]
        [TabGroup("Owner Setting")]
        public GetFromType _getFromType = GetFromType.ParentVarOwner;

        // public override Type GetValueType => typeof(TValueType);

        [ShowInDebugMode]
        private MonoBehaviour CurrentTarget
        {
            get
            {
                if (_currentTarget == null)
                    return this;
                return _currentTarget;
            }
        }

        private MonoBehaviour _currentTarget;

        private bool TypeCheckFail()
        {
            if (_varTag == null)
                return false;
            return typeof(TValueType).IsAssignableFrom(_varTag._valueFilterType.RestrictType)
                == false;
        }

        // [ValueDropdown(nameof(GetGlobalMonoTags))] [OnValueChanged(nameof(OnGlobalMonoTagChange))]
        //FIXME: 常常會空著?
        //globalTag
        //a(object).b(variable)
        //VariableOwner的話就可以往parent找，不是的話可以從asset找？ auto assign? 或是根本不需要
        [FormerlySerializedAs("_parentMonoTag")]
        [TabGroup("Owner Setting")]
        [HideIf(nameof(IsFromParentOwner))]
        public MonoEntityTag _blackboardTag; //空的話就是自己

        private bool IsFromParentOwner()
        {
            if (_getFromType == GetFromType.ParentVarOwner)
                return true;
            return false;
        }

        [BoxGroup("varTag")]
        [ShowInInspector]
        [ValueDropdown(nameof(GetParentVariableTags), NumberOfItemsBeforeEnablingSearch = 5)]
        private VariableTag DropDownVarTag
        {
            set
            {
                _varTag = value;
                // 當變數標籤改變時，重新更新路徑條目的型別
                OnPathEntriesChanged();
            }
            get => _varTag;
        }

        [BoxGroup("varTag")]
        [GUIColor(0.8f, 1.0f, 0.8f)]
        // [PreviewInInspector]
        [ShowInInspector]
        [DisableIf(nameof(_varTag))]
        public TVarMonoType Variable
        {
            get => VarRaw as TVarMonoType;
            set =>
                //fixme: 自動爬出tag
                _varTag = value._varTag;
            //mono?
        }

        //FIXME: dropdown validate? 多檢查parent的owner? dropdown tag?
        [ShowInDebugMode]
        [BoxGroup("varTag")]
        [FormerlySerializedAs("varTag")]
        [InfoBox("Tag Type is wrong", InfoMessageType.Error, nameof(TypeCheckFail))]
        [Required]
        public VariableTag _varTag;

        private void OnVarOwnerChange()
        {
            var _ = owner;
            Debug.Log("OnVarOwnerChange" + owner, owner);
            if (owner)
                _blackboardTag = owner.DefaultTag; //需要set dirty嗎？
        }

        [OnValueChanged(nameof(OnVarOwnerChange))]
        [ShowIf(nameof(_getFromType), GetFromType.VariableOwnerProvider)]
        [CompRef]
        // [Component(AddComponentAt.Same)]
        [Auto]
        [TabGroup("Owner Setting")]
        [Required]
        //開prefab
        public IEntityValueProvider entityProvider;

        public MonoEntity entity => _varEntity != null ? _varEntity.Value : entityProvider.Value;

        public VarEntity _varEntity;

        private IEnumerable<ValueDropdownItem<VariableTag>> GetParentVariableTags() //editor time?
        {
            var tagDropdownItems = new List<ValueDropdownItem<VariableTag>>();
            switch (_getFromType)
            {
                case GetFromType.VariableOwnerProvider:

                    if (entity == null)
                        return tagDropdownItems;
                    if (Application.isPlaying)
                    {
                        var variables = entity.VariableFolder.GetValues;
                        foreach (var variable in variables)
                            if (variable is TVarMonoType)
                                tagDropdownItems.Add(
                                    new ValueDropdownItem<VariableTag>(
                                        variable.name,
                                        variable._varTag
                                    )
                                );
                    }
                    else
                    {
#if UNITY_EDITOR
                        var tags = _blackboardTag.containsVariableTypeTags;
                        foreach (var varTag in tags)
                            tagDropdownItems.Add(
                                new ValueDropdownItem<VariableTag>(varTag.name, varTag)
                            );
#endif
                    }

                    break;

                case GetFromType.GlobalInstance:

                    // Debug.Log("GetFromType.GlobalInstance", this);
                    var instance = CurrentTarget.GetGlobalInstance(_blackboardTag);
                    if (instance == null)
                    {
                        //從MonoDescriptableTag找到varTag (schema一定會一致嗎？不一定)
                        var parentMonoVarTags = _blackboardTag.containsVariableTypeTags;
                        //FIXME: 有 null 要清掉

                        foreach (var parentVarTag in parentMonoVarTags)
                        {
                            Debug.Log(
                                "TValueType: "
                                    + typeof(TValueType)
                                    + " restrictType: "
                                    + parentVarTag._valueFilterType.RestrictType,
                                this
                            );
                            if (
                                typeof(TValueType).IsAssignableFrom(
                                    parentVarTag._valueFilterType.RestrictType
                                )
                            )
                                tagDropdownItems.Add(
                                    new ValueDropdownItem<VariableTag>(
                                        parentVarTag.name,
                                        parentVarTag
                                    )
                                );
                        }
                        return tagDropdownItems;
                    }

                    Debug.Log("TVarMonoType: " + typeof(TVarMonoType));

                    //從instance直接找variable
                    foreach (var variable in instance.VariableFolder.GetValues)
                    {
                        if (variable is TVarMonoType)
                            tagDropdownItems.Add(
                                new ValueDropdownItem<VariableTag>(variable.name, variable._varTag)
                            );
                    }

                    break;
                case GetFromType.ParentVarOwner:
                {
                    var parents = CurrentTarget.GetComponentsInParent<MonoBlackboard>();

                    foreach (var parent in parents)
                    {
                        if (parent.VariableFolder == null)
                        {
                            Debug.LogError("Parent VariableFolder is null", parent);
                            continue;
                        }

                        foreach (var variable in parent.VariableFolder.GetValues)
                            if (variable is TVarMonoType)
                                tagDropdownItems.Add(
                                    new ValueDropdownItem<VariableTag>(
                                        variable.name,
                                        variable._varTag
                                    )
                                );
                    }

                    if (tagDropdownItems.Count == 0)
                    {
                        Debug.LogError("All Parent VariableFolder has no Variable", CurrentTarget);
                        foreach (var parent in parents)
                            Debug.LogError(
                                $"Parent {parent} has no Variable?"
                                    + parent.VariableFolder
                                    + parent.VariableFolder.GetValues.Count,
                                parent
                            );
                    }

                    break;
                }
            }

            return tagDropdownItems;
        }

        // private IEnumerable<ValueDropdownItem<MonoDescriptableTag>> GetParentMonoTags()
        // {
        //     var parents = CurrentTarget.GetComponentsInParent<MonoDescriptable>();
        //     var tags = new List<ValueDropdownItem<MonoDescriptableTag>>();
        //     foreach (var parent in parents)
        //         tags.Add(new ValueDropdownItem<MonoDescriptableTag>(parent.Tag.name, parent.Tag));
        //
        //     return tags;
        // }


        [ShowInDebugMode]
        [PreviewInInspector]
        private Type variableValueType => typeof(TValueType);

        [ShowInDebugMode]
        public MonoBlackboard owner
        {
            get
            {
                if (Application.isPlaying && _runtimeCachedOwner != null) //runtime才要cache
                    return _runtimeCachedOwner;

                _runtimeCachedOwner = FetchOwner(CurrentTarget);
                return _runtimeCachedOwner;
            }
        }

        private MonoBlackboard FetchOwner(MonoBehaviour target)
        {
            if (target == null)
            {
                if (Application.isPlaying)
                    Debug.LogError("Target is null", this);
                return null;
            }

            if (_getFromType == GetFromType.VariableOwnerProvider)
            {
                entityProvider = GetComponent<IEntityValueProvider>();
                // if (_entityProvider == null)
                //     // Debug.LogError("VariableOwnerProvider is null", this);
                //     return null;
                if (entity == null)
                {
                    if (Application.isPlaying)
                        Debug.LogError("VariableOwnerProvider.GetVariableOwner is null", this);
                    return null;
                }

                return entity;
            }

            if (_blackboardTag != null)
            {
                //FIXME:  不對
                var monoCompInParent = target.GetMonoCompInParent(_blackboardTag);
                if (monoCompInParent == null)
                    return null;

                return monoCompInParent;
            }

            //FIXME: 這個會爆掉？
            _runtimeCachedOwner = target.GetComponentInParent<MonoBlackboard>();
            if (Application.isPlaying)
                if (_runtimeCachedOwner == null)
                    Debug.LogError("VariableOwner InParent is null at:", target);

            return _runtimeCachedOwner;
        }

        private MonoBlackboard _runtimeCachedOwner;

        // public void SetValue(TValueType value, MonoBehaviour byWho)
        // {
        //     VarRaw.SetValue(value, byWho);
        // }

        public override TMonoVar GetVar<TMonoVar>()
        {
            return VarRaw as TMonoVar;
        }

        public override AbstractMonoVariable VarRaw
        {
            get
            {
                //FIXME: 抽掉？
                if (_getFromType == GetFromType.GlobalInstance)
                {
                    var descriptable = CurrentTarget.GetGlobalInstance(_blackboardTag);
                    if (descriptable == null)
                        return null;
                    return descriptable.GetVar(_varTag);
                }

                if (_getFromType == GetFromType.VariableOwnerProvider)
                {
                    if (Application.isPlaying == false)
                        return null;

                    Debug.Log("_getFromType == GetFromType.VariableOwnerProvider", this);

                    // if (_entityProvider == null)
                    //     return null;
                    if (entity == null)
                    {
                        Debug.LogError("entity null", this);
                        return null;
                    }

                    return entity.GetVar(_varTag);
                }

                if (owner == null)
                {
                    if (Application.isPlaying)
                        Debug.LogError("Owner is null", CurrentTarget);
                    return null;
                }

                if (owner.VariableFolder == null)
                {
                    if (Application.isPlaying)
                        Debug.LogError("VariableFolder is null", CurrentTarget);
                    return null;
                }

                var variable = owner.GetVar(_varTag);
                if (Application.isPlaying)
                    if (variable == null)
                        Debug.LogError($"Variable{_varTag} is null in owner{owner}", CurrentTarget);

                return variable;
            }
        }

        public override T1 Get<T1>() //原本interface就做掉了，但是
        {
            var value = Value;
            Profiler.BeginSample("VariableProviderRef.Get cast", this);
            var t1Value = Unsafe.As<TValueType, T1>(ref value);
            Profiler.EndSample();
            return t1Value;
            // if (value is T1 t1Value)
            // {
            //     Profiler.EndSample();
            //     return t1Value;
            // }


            Debug.LogError(
                $"無法將欄位值 {value} (型別: {value.GetType()}) 轉換為 {typeof(T1)}",
                this
            );
            return default;
        }

        [ShowInDebugMode]
        [PreviewInInspector]
        public TValueType Value
        {
            get
            {
                if (VarRaw == null)
                    return default;

                // 如果沒有設定欄位路徑，直接回傳變數值
                if (!HasFieldPath)
                    return VarRaw.GetValue<TValueType>();

                // 使用欄位路徑存取特定欄位值
                var (fieldValue, info) = ReflectionUtility.GetFieldValueFromPath<TValueType>(
                    VarRaw,
                    _pathEntries,
                    gameObject
                );

                if (fieldValue != null)
                    return fieldValue;

                // 嘗試轉型
                // if (fieldValue != null)
                //     try
                //     {
                //         return (TValueType)Convert.ChangeType(fieldValue, typeof(TValueType));
                //     }
                //     catch (Exception e)
                //     {
                //         if (Application.isPlaying)
                //             Debug.LogError(
                //                 $"無法將欄位值 {fieldValue} (型別: {fieldValue.GetType()}) 轉換為 {typeof(TValueType)}: {e.Message}",
                //                 this
                //             );
                //     }

                return default;
            }
        }

        public override VariableTag varTag => _varTag;

        // set => _varTag = value;
        // public object GetValue()
        // {
        //     if (VarRaw == null) return null;
        //
        //     // 如果沒有設定欄位路徑，直接回傳變數值
        //     if (!HasFieldPath) return VarRaw.GetValue<TValueType>();
        //
        //     // 使用欄位路徑存取特定欄位值
        //     return ReflectionUtility.GetFieldValueFromPath(VarRaw, pathEntries, gameObject);
        // }
        //
        // public T GetValue<T>()
        // {
        //     var value = GetValue();
        //     switch (value)
        //     {
        //         case null:
        //             return default;
        //         case T value1:
        //             return value1;
        //         default:
        //             // 嘗試轉型
        //             try
        //             {
        //                 return (T)Convert.ChangeType(value, typeof(T));
        //             }
        //             catch (Exception)
        //             {
        //                 if (Application.isPlaying)
        //                     Debug.LogError($"Cannot cast {value} to {typeof(T)}", this);
        //                 return default;
        //             }
        //     }
        // }

        public override Type ValueType => typeof(TValueType);

        public override string Description
        {
            get
            {
                var str = string.Empty;
                if (_blackboardTag)
                    str = _blackboardTag.name + ".";
                str += varTag?.name;
                if (HasFieldPath)
                    str += "." + string.Join(".", _pathEntries.Select(e => e._propertyName));
                return str;
            }
        }
    }
}

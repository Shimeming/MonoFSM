using System;
using System.Collections.Generic;
using System.Reflection;
using MonoFSM.Core;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.DataProvider;
using MonoFSM.CustomAttributes;
using MonoFSM.EditorExtension;
using MonoFSM.Foundation;
using MonoFSM.Runtime.Variable;
using MonoFSM.Variable.Attributes;
using MonoFSM.Variable.VariableBinder;
using MonoFSM.VarRefOld;
using MonoFSMCore.Runtime.LifeCycle;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace MonoFSM.Variable
{
    public abstract class TypedMonoVariable<T> : AbstractMonoVariable, ISettable<T>
    {
        [CompRef]
        [AutoChildren] //fixme; 用interface的壞處？需要 ensurecomponent? 但不會髒掉？
        protected IValueProvider[] _valueSources; //FIXME: 好像可以改成AbstractValueProvider<T>?

        protected IValueProvider valueSource => GetActiveTypedValueSource();

        protected IValueProvider GetActiveTypedValueSource()
        {
            this.EnsureComponentInChildren(ref _valueSources);
            return ValueResolver.GetActiveValueSource(_valueSources, this);
        }

        protected override bool HasValueProvider
        {
            get
            {
                this.EnsureComponentsInChildren(ref _valueSources); //要避免Array?
                return ValueResolver.HasValueProvider(_valueSources) || base.HasValueProvider;
            }
        }

        public abstract void SetValue(T value, MonoBehaviour byWho);
        public abstract void SetValue(object value, MonoBehaviour byWho);

        public abstract void CommitValue();
    }

    //FIXME: 應該要繼承AbstractSourceValueRef
    public abstract class AbstractMonoVariable //Rename self?
        : MonoBehaviour,
            IGuidEntity,
            IName,
            IValueOfKey<VariableTag>,
            IOverrideHierarchyIcon,
            IBeforePrefabSaveCallbackReceiver,
            IConfigTypeProvider,
            IResetStateRestore
    {
        //FIXME: 什麼case需要parentVarEntity? 忘記了XD
        // [ShowIf(nameof(_parentVarEntity))] //有才顯示就好, 或是debugMode?
        [PreviewInInspector]
        [AutoParent(includeSelf: false)] //不可以抓到自己！
        protected VarEntity _parentVarEntity; //我的parent如果有VarEntity, 去跟這個entity拿？

        public bool HasParentVarEntity
        {
            get
            {
                //如果是null這個會很白痴耶
                this.EnsureComponentInParent(ref _parentVarEntity, false, false);
                return _parentVarEntity != null;
            }
        }
#if UNITY_EDITOR
        public string IconName { get; }
        public bool IsDrawingIcon => CustomIcon != null;

        public Texture2D CustomIcon =>
            EditorGUIUtility.ObjectContent(null, GetType()).image as Texture2D; //雞掰！
        //UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.rcgmaker.fsm/RCGMakerFSMCore/Runtime/2_Variable/VarFloatIcon.png");
#endif

        //FIXME: 這個不能被Debug「看」，不好用... AddListener 的形式比較好
        // private UnityAction OnValueChangedRaw; //任何數值改變就通知, UI有用到很重要 //override?

        private HashSet<IVarChangedListener> _dataChangedListeners; //有誰有用我，binder綁一下

        public abstract void ClearValue();

        //fuck!?

        //倒著，事件鏈超難trace
        public void OnValueChanged()
        {
            if (!Application.isPlaying)
                return;
            if (_dataChangedListeners != null)
                foreach (var item in _dataChangedListeners)
                    item.OnVarChanged(this);
            // OnValueChangedRaw?.Invoke();
            // Debug.Log("OnValueChanged", this);
        }

        public void AddListener(IVarChangedListener target)
        {
            if (_dataChangedListeners == null)
                _dataChangedListeners = new HashSet<IVarChangedListener>();
            _dataChangedListeners.Add(target);
        }

        public void RemoveListener(IVarChangedListener target)
        {
            _dataChangedListeners.Remove(target);
        }

        [AutoParent]
        protected VariableFolder _variableFolder;

        // [Button]
        private void UpdateTag()
        {
            if (_varTag != null)
            {
                _varTag._variableType.SetType(GetType());

                //如果有了不該蓋掉？如果改型別了呢？還是要看有沒有繼承關係？
                //FIXME: BaseFilterType應該要改？
                if (_varTag.ValueFilterType == null)
                    _varTag._valueFilterType.SetType(ValueType);
            }

            // Debug.Log("Tag Changed");
            //variable folder refresh
            _variableFolder = GetComponentInParent<VariableFolder>();
            if (_variableFolder)
                _variableFolder.Refresh();
#if UNITY_EDITOR
            if (_varTag)
                EditorUtility.SetDirty(_varTag);
#endif
        }

        //         [Button("建立 ValueProvider Reference")]
        //         private void CreateValueProvider()
        //         {
        // #if UNITY_EDITOR
        //             if (_varTag == null)
        //             {
        //                 Debug.LogError("請先設定變數標籤 (VarTag) 才能建立 ValueProvider", this);
        //                 return;
        //             }
        //
        //             // 加入 ValueProvider 組件
        //             var valueProvider = gameObject.TryGetCompOrAdd<ValueProvider>();
        //
        //             valueProvider.DropDownVarTag = _varTag; //直接設定
        //
        //             // 設定 ValueProvider 的 EntityProvider
        //             valueProvider._entityProvider = GetComponentInParent<ParentEntityProvider>();
        //             // 標記為 dirty 以確保儲存
        //             EditorUtility.SetDirty(valueProvider);
        //
        // #else
        //             Debug.LogWarning("此功能僅在編輯器模式下可用");
        // #endif
        //         }

        //         [Button("建立 ValueProvider Reference In Children")]
        //         private void CreateValueProviderInChildren()
        //         {
        // #if UNITY_EDITOR
        //             if (_varTag == null)
        //             {
        //                 Debug.LogError("請先設定變數標籤 (VarTag) 才能建立 ValueProvider", this);
        //                 return;
        //             }
        //
        //             // 加入 ValueProvider 組件
        //             var valueProvider = gameObject.AddChildrenComponent<ValueProvider>("provider");
        //
        //             valueProvider.DropDownVarTag = _varTag; //直接設定
        //
        //             // 設定 ValueProvider 的 EntityProvider
        //             valueProvider._entityProvider = GetComponentInParent<ParentEntityProvider>();
        //             // 標記為 dirty 以確保儲存
        //             EditorUtility.SetDirty(valueProvider);
        //
        // #else
        //             Debug.LogWarning("此功能僅在編輯器模式下可用");
        // #endif
        //         }

        //proxy variable or local variable;
        protected bool IsHidingVarTag => HasValueProvider || _variableFolder == null;

        protected bool IsHidingDefaultValue =>
            HasValueProvider || HasParentVarEntity || _variableFolder == null;

        //是一種Object Member的概念？
        [HideIf(nameof(IsHidingVarTag))]
        [FormerlySerializedAs("varTag")]
        // [MCPExtractable]
        [OnValueChanged(nameof(UpdateTag))]
        [Header("變數名稱")]
        [PropertyOrder(-1)]
        [Required]
        [SOConfig("VariableType", nameof(CreateTagPostProcess))]
        public VariableTag _varTag; //直接看當下是什麼就可以 好像可以再往下抽？ ValueContainer? , readonly => Config, settable

        protected void CreateTagPostProcess()
        {
            //FIXME: 從Drawer call 失敗了，感覺varTag還沒做好...
            // varTag._variableType.SetType(GetType());
            // varTag._valueFilterType.SetType(ValueType);
            // Debug.Log("CreateTagPostProcess" + varTag._variableType.RestrictType + varTag._valueFilterType.RestrictType,
            //     varTag);
        }

        public T1 Get<T1>()
        {
            return GetValue<T1>();
        }

        public virtual Type ValueType => _varTag.ValueType; //遞回了ㄅ？

        //FIXME: 好亂喔QQ 好難trace
        public abstract object objectValue { get; } //不好？generic value?

        public virtual T GetValue<T>()
        {
            var value = objectValue;
            if (value == null)
                return default;
            try
            {
                return (T)value;
            }
            catch (Exception e)
            {
                Debug.LogError($"Cannot cast {value} to {typeof(T)}", this);
                return default;
            }
        }

        // private readonly HashSet<Object> byWhoHashSet = new();
        // [ShowInDebugMode] public List<Object> byWhoList => byWhoHashSet.ToList();
        protected abstract void SetValueInternal<T>(T value, Object byWho);

#if UNITY_EDITOR
        [ShowInDebugMode]
        private Queue<SetValueExecutionData> _byWhoQueue = new(); //沒有人清，resetrestore要清掉嗎？先不要好了

        [Serializable]
        public struct SetValueExecutionData
        {
            public object _value; //可能被attribute processor給處理到，好像有點太過侵入？
            public Object _byWho;
            public float _time;
        }
#endif

        protected void RecordSetbyWho<T>(Object byWho, T tempValue)
        {
            // byWho.Log("[Variable] Set", name, value);
            this.Log("[Variable] Set", tempValue, "byWho", byWho);
            // byWhoHashSet.Add(byWho);
#if UNITY_EDITOR
            //這個最係最好，如果有包含原因會更好？直接用字串？
            var byWhoData = new SetValueExecutionData
            {
                _value = tempValue,
                _byWho = byWho,
                _time = Time.time,
            };
            _byWhoQueue.Enqueue(byWhoData);
            if (_byWhoQueue.Count > 10)
                _byWhoQueue.Dequeue(); //保持最新的10個
#endif
        }

        public void SetValue<T>(T value, Object byWho)
        {
            if (_parentVarEntity != null) //用proxy
            {
                if (_parentVarEntity != this)
                {
                    Debug.Log("Proxy SetValue to parent entity", _parentVarEntity);
                    var targetVar = _parentVarEntity.Value.GetVar(_varTag);
                    if (targetVar == null)
                    {
                        Debug.LogError(
                            $"Parent entity {_parentVarEntity.name} has no var {_varTag.name}",
                            this
                        );
                        return;
                    }

                    if (targetVar == this)
                    {
                        Debug.LogError(
                            "Variable's parent entity is self, possible misconfiguration.",
                            this
                        );
                        Debug.Break();
                        return;
                    }

                    targetVar.SetValue(value, byWho);

                    return;
                }
                else
                {
                    Debug.LogError(
                        "Variable's parent entity is self, possible misconfiguration.",
                        this
                    );
                }

                Debug.Break();
            }

            SetValueInternal(value, byWho);
            OnValueChanged();

            // OnValueChangedRaw?.Invoke(); //通知有人改變了
            //FIXME: 如果還有什麼需要處理的？
        }

        public bool Equals(AbstractSourceValueRef sourceValueRef)
        {
            if (sourceValueRef == null)
            {
                Debug.LogError("Equals: sourceValueRef is null", this);
                return false;
            }

            var type = sourceValueRef.ValueType;
            if (type == typeof(int))
                return Equals(sourceValueRef.GetValue<int>());
            if (type == typeof(float))
                return Equals(sourceValueRef.GetValue<float>());
            if (type == typeof(bool))
                return Equals(sourceValueRef.GetValue<bool>());
            if (type == typeof(string))
                return Equals(sourceValueRef.GetValue<string>());
            if (type == typeof(Vector3))
                return Equals(sourceValueRef.GetValue<Vector3>());
            if (typeof(Object).IsAssignableFrom(type))
                return Equals(sourceValueRef.GetValue<Object>());
            Debug.LogWarning($"Equals: Unsupported type {type}", this);
            return Equals(sourceValueRef.GetValue<object>());
        }

        public bool Equals<T>(T value)
        {
            var v = GetValue<T>();
            return EqualityComparer<T>.Default.Equals(v, value);
        }

        public void SetValueByValueProvider(IValueProvider provider, Object byWho)
        {
            if (provider == null)
            {
                Debug.LogError("SetValueByValueProvider: provider is null", this);
                return;
            }

            var type = provider.ValueType;

            if (type == typeof(int))
            {
                SetValue(provider.Get<int>(), byWho);
                return;
            }

            if (type == typeof(float))
            {
                SetValue(provider.Get<float>(), byWho);
                return;
            }

            if (type == typeof(string))
            {
                SetValue(provider.Get<string>(), byWho);
                return;
            }

            if (type == typeof(bool))
            {
                SetValue(provider.Get<bool>(), byWho);
                return;
            }

            if (type == typeof(Vector2))
            {
                SetValue(provider.Get<Vector2>(), byWho);
                return;
            }

            if (type == typeof(Vector3))
            {
                SetValue(provider.Get<Vector3>(), byWho);
                return;
            }

            if (type == typeof(Vector4))
            {
                SetValue(provider.Get<Vector4>(), byWho);
                return;
            }

            if (type == typeof(Quaternion))
            {
                SetValue(provider.Get<Quaternion>(), byWho);
                return;
            }

            if (typeof(Object).IsAssignableFrom(type))
            {
                SetValue(provider.Get<Object>(), byWho);
                return;
            }

            Debug.LogError("SetValueByValueProvider: Unsupported type " + type, this);
            SetValue(provider.Get<object>(), byWho);
        }

        public void SetValueByRef(AbstractSourceValueRef sourceValueRef, Object byWho)
        {
            if (sourceValueRef == null)
            {
                Debug.LogError("SetValue: sourceValueRef is null", this);
                return;
            }

            var type = sourceValueRef.ValueType;

            if (type == typeof(int))
            {
                SetValue(sourceValueRef.GetValue<int>(), byWho);
                return;
            }

            if (type == typeof(float))
            {
                SetValue(sourceValueRef.GetValue<float>(), byWho);
                return;
            }

            if (type == typeof(string))
            {
                SetValue(sourceValueRef.GetValue<string>(), byWho);
                return;
            }

            if (type == typeof(bool))
            {
                SetValue(sourceValueRef.GetValue<bool>(), byWho);
                return;
            }

            if (type == typeof(Vector2))
            {
                SetValue(sourceValueRef.GetValue<Vector2>(), byWho);
                return;
            }

            if (type == typeof(Vector3))
            {
                SetValue(sourceValueRef.GetValue<Vector3>(), byWho);
                return;
            }

            if (type == typeof(Vector4))
            {
                SetValue(sourceValueRef.GetValue<Vector4>(), byWho);
                return;
            }

            if (type == typeof(Quaternion))
            {
                SetValue(sourceValueRef.GetValue<Quaternion>(), byWho);
                return;
            }

            if (typeof(Object).IsAssignableFrom(type))
            {
                SetValue(sourceValueRef.GetValue<Object>(), byWho);
                return;
            }

            Debug.LogError("SetValue: Unsupported type " + type, this);
            SetValue(sourceValueRef.GetValue<object>(), byWho);
        }

        public object GetProperty(string knownFieldName)
        {
            return GetPropertyCache(knownFieldName)?.Invoke(this);
        }

        public Dictionary<string, Func<AbstractMonoVariable, object>> _propertyCache = new();

        //GameFlagDescriptable有一樣的東西喔
        public Func<AbstractMonoVariable, object> GetPropertyCache(string propertyName)
        {
            if (_propertyCache.TryGetValue(propertyName, out var info))
                return info;

            var propertyInfo = GetType()
                .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

            // Debug.Log($"Property {propertyName} found in {sourceObject.GetType()}", sourceObject);

            if (propertyInfo == null)
            {
                _propertyCache[propertyName] = null;
                //FIXME: 可能因為unknownData所以有可能會找不到 有點危險？
                // Debug.LogError($"Property {propertyName} not found in {GetType()}");
                return null;
            }

            var getMethod = propertyInfo.GetGetMethod();
            if (getMethod == null)
            {
                Debug.LogError($"Property {propertyName} does not have a getter in {GetType()}");
                return null;
            }

            Func<AbstractMonoVariable, object> getMyProperty = (source) =>
                getMethod.Invoke(source, null);
            _propertyCache[propertyName] = getMyProperty;
            return getMyProperty;
        }

#if UNITY_EDITOR
        // [Header("GameState 功能說明")]
        //FIXME: 整合 AbstractDescriptable??
        [TextArea(1, 4)]
        public string description;

        public string Description
        {
            get => description;
            set => description = value;
        }
#endif

        // [HideInInlineEditors] [Header("Flag Setting")]
        // public FlagTypeScriptable typeScriptable;
        protected virtual void Awake()
        {
            if (_parentVarEntity != null)
                Debug.Log("Variable has parent entity: " + _parentVarEntity.name, this);
        } //FIXME: 好像盡量不要亂用awake喔

        //FIXME: virtual variable?
        // [FormerlySerializedAs("VariableSource")]
        // [ShowIf("VariableSource")]
        // [InlineEditor] public AbstractMonoVariable VariableSource; //用別人的值 //FIXME: 什麼時候會用到這個？

        // [ReadOnly]
        // public List<AbstractVariableConsumer> consumers; //有誰有用我，binder綁一下

        //FIXME: 這個是錯的，要改成用scriptableData的 (flagFlied的？
        // public UnityEvent ValueChangedEvent => valueChangedEvent;

        // [HideInInlineEditors] public UnityEvent valueChangedEvent;
        public string Name => gameObject.name;
        public VariableTag Key => _varTag;
        public abstract bool IsValueExist { get; }
        protected virtual bool HasValueProvider => false;
        protected virtual bool HasProxyValue => HasValueProvider || HasParentVarEntity;

        public VariableTag[] GetKeys()
        {
            return new[] { _varTag };
        }

        // public virtual void OnBeforePrefabSave()
        // {
        //
        // }

        [Button("Save")]
        public virtual void OnBeforePrefabSave()
        {
            // base.OnBeforePrefabSave();
            UpdateTag();
            //FIXME: 不知道怎麼做比較好的自動取名
            // if (HasValueProvider)
            // {
            //     name = "=>"; //hmm多個怎麼辦...
            //     return;
            // }

            if (_varTag == null)
            {
                Debug.LogError("No VarTag: " + this, this);
                return;
            }

            var str = _varTag.name;
            if (_parentVarEntity != null)
                str = _parentVarEntity.name + "_" + str;

            name = str;
            //FIXME: 怎麼更好的取名？
        }

        public Type GetRestrictType()
        {
            return _varTag?.ValueFilterType;
        }

        public abstract void ResetStateRestore();
        // public abstract void ResetToDefaultValue();
    }
}

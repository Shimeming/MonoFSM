using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using _1_MonoFSM_Core.Runtime._1_States;
using JetBrains.Annotations;
using MonoFSM.Core;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.DataProvider;
using MonoFSM.Core.Simulate;
using MonoFSM.Runtime.Interact.EffectHit;
using MonoFSM.Runtime.Mono;
using MonoFSM.Runtime.Variable;
using MonoFSM.Variable;
using MonoFSM.Variable.FieldReference;
using MonoFSMCore.Runtime.LifeCycle;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace MonoFSM.Runtime
{
    public interface IMonoAddToBinderChecker //network想要看authority來決定要不要加到字典裡...這個性質是什麼
    {
        bool IsAddValid();
    }

    //FIXME: 必定需要MonoObj?
    [RequireComponent(typeof(MonoObj))]
    [Searchable]
    [FormerlyNamedAs("MonoDescriptable")]
    public class MonoEntity
        : AbstractMonoDescriptable<GameData>,
            IInstantiated,
            IBeforePrefabSaveCallbackReceiver,
            IGameDataProvider,
            IValueProvider,
            IAnimatorProvider //這樣data也要一直繼承，好ㄇ...
    {
        //FIXME: GetComponentsInChildren IFeature, 然後用type把dict包起來，那和schema不就一樣了？更彈性的版本嗎？

        [PreviewInInspector]
        [AutoChildren(DepthOneOnly = true)]
        private SchemaFolder _schemaFolder;

        // set => _entitySchema = value;
        //Utility? 開始要拿一些Rigidbody、Collider之類的東西了
        // public Rigidbody DefaultRigidbody => GetComponent<Rigidbody>(); //FIXME: 位置對嗎？

        //FIXME: nested? MonoEntity dictionary?
        public void OnInstantiated(WorldUpdateSimulator world)
        {
            //network要看authoring... network版的？啥？ NetworkMonoDescriptableBinder?
            //想要註冊世界了，顆顆
            //掉在外面就不能註冊了，binder是不是不好？
            //從world去bind?
            //FIXME: 哪些要註冊？localplayer才需要？
            _worldBinder = world.GetComponent<MonoEntityBinder>();
            if (_worldBinder)
            {
                Debug.Log("Registering MonoEntity to WorldBinder: " + name, this);
                _worldBinder.Add(DefaultTag, this); //註冊法
            }
            else
                Debug.LogError(
                    "MonoDescriptableBinder not found in parent, cannot register to world binder",
                    this
                );
            // GetComponent<MonoDescriptableBinder>().Add(DescriptableTag, this);
        }

        [PreviewInInspector]
        MonoEntityBinder _worldBinder;

        public void OnBeforePrefabSave()
        {
            FillVarTagsToMonoDescriptableTag();
        }

        public GameData GameData => Data;

        public T1 Get<T1>()
        {
            if (this is T1 t1)
                return t1;

            Debug.LogError($"Cannot cast {GetType()} to {typeof(T1)}", this);
            return default;
        }

        public Type ValueType => GetType();
        public string Description { get; }
        public Animator ChildAnimator => GetComponentInChildren<Animator>();

        public Animator[] ChildAnimators => GetComponentsInChildren<Animator>();

        public TSchema GetSchema<TSchema>()
            where TSchema : AbstractEntitySchema
        {
            if (_schemaFolder != null)
            {
                var result = _schemaFolder.Get<TSchema>();
                return result;
            }

            // if (_entitySchema is TSchema schema) return schema;

            Debug.LogError($"Schema {typeof(TSchema)} not found in {name}", this);
            return null;
        }
    }

    //描述物件的monoNode, Entity? MonoEntity?
    //場景物件、角色、
    //應該要可以繼承這個嗎？Inventory
    //不該有variable嗎？
    //FIXME: 這層多餘嗎？
    public class AbstractMonoDescriptable<TMonoDescriptable>
        : MonoBlackboard,
            IMonoDescriptable,
            ISceneAwake
        where TMonoDescriptable : GameData //,IVariableOwner //VariableOwner?
    {
        //FIXME: 更複雜的描述組合？
        [UsedImplicitly] //從UI直接選
        public virtual string RuntimeDescription =>
            string.IsNullOrEmpty(Data.Description) ? Data.name : Data.Description;

        // #if UNITY_EDITOR
        //         [RequiredIn(PrefabKind.InstanceInScene)] [PreviewInInspector] [AutoParent]
        //         private MonoDescriptableBinder _binder;
        // #endif

        //GameLogic不該Nested?
        //FIXME: 太深了...會包到過多的東西
        [PreviewInInspector]
        [AutoChildren]
        private GeneralEffectDealer[] _dealers; //可以互動的性質門

        // private HashSet<GeneralEffectType> _dealerTypeSet = new HashSet<GeneralEffectType>(); //可以被互動的性質

        private readonly Dictionary<GeneralEffectType, GeneralEffectDealer> _dealerTypeMap = new(); //keys?
#if UNITY_EDITOR

        [PreviewInInspector]
        private List<GeneralEffectType> DealerTypes => _dealerTypeMap.Keys.ToList();

        [PreviewInInspector]
        private List<GeneralEffectType> ReceiverTypes => _receiverTypeMap.Keys.ToList();
#endif

        [PreviewInInspector]
        private int DealerSetCount => _dealerTypeMap.Count;

        /// <summary>
        ///     直接讓兩個entity觸發作用
        /// </summary>
        /// <param name="effectType"></param>
        /// <param name="target"></param>
        public void ApplyEffectTo(GeneralEffectType effectType, MonoEntity target)
        {
            //FIXME: 可以保證一個entity下只有一個type的Dealer嗎？不一定吧？但receiver可以？ 還是拿到list?
            if (target == null)
            {
                Debug.LogError("Target is null", this);
                return;
            }

            var dealer = GetDealer(effectType);
            var receiver = target.GetReceiver(effectType);
            receiver.ForceDirectEffectHit(dealer);
        }

        [PreviewInInspector]
        [AutoChildren]
        private GeneralEffectReceiver[] _receivers; //可以互動的性質門

        // readonly HashSet<GeneralEffectType> _receiverTypeSet = new HashSet<GeneralEffectType>(); //可以被互動的性質
        private readonly Dictionary<GeneralEffectType, GeneralEffectReceiver> _receiverTypeMap =
            new();

        [PreviewInInspector]
        private int ReceiverSetCount => _receiverTypeMap.Count;

        //帶有xx性質的物件
        public bool HasReceiverType(GeneralEffectType effectType)
        {
            return _receiverTypeMap.ContainsKey(effectType);
            // return _receiverTypeSet.Contains(effectType);
        }

        public bool HasDealerType(GeneralEffectType effectType)
        {
            return _dealerTypeMap.ContainsKey(effectType);
            // return _dealerTypeSet.Contains(effectType);
        }

        public GeneralEffectDealer GetDealer(GeneralEffectType effectType)
        {
            if (_dealerTypeMap.TryGetValue(effectType, out var dealer) == false)
            {
                Debug.LogError($"Dealer {effectType} not found in {name}", this);
                return null;
            }

            return dealer;
        }

        public GeneralEffectReceiver GetReceiver(GeneralEffectType effectType)
        {
            if (effectType == null)
            {
                Debug.LogError("EffectType is null", this);
                return null;
            }
            if (_receiverTypeMap.TryGetValue(effectType, out var receiver) == false)
            {
                Debug.LogError($"Receiver \"{effectType}\" not found in {name}", this);
                return null;
            }

            return receiver;
        }

        // public DescriptableData SampleData;
        //FIXME: 不一定需要data? VarGameData比較對？這樣就往下直接找，code就從schema define? 還是其實 MonoEntity要和Schema合併
        [SOConfig("10_Flags/GameData")]
        [SerializeField]
        protected TMonoDescriptable data; //config

        public virtual IDescriptableData Descriptable => data;

        public T GetData<T>()
            where T : GameData
        {
            return data as T;
        }

        // [ShowInInspector]
        public TMonoDescriptable Data
        {
            get => data;
            // set => data = value;
        }

        // [SerializeField] private MonoDescriptableTag[] DescriptableTags; //

        public virtual void OnUIEventReceived() //FIXME; 這啥XD
        {
            Debug.Log("UI Event Received", this);
        }

        // public object GetValue(VariableTag varTag)
        // {
        //     var variable = VariableFolder.GetVariable(varTag);
        //     if (variable == null)
        //     {
        //         Debug.LogError($"Variable {varTag} not found in {name}", this);
        //         return null;
        //     }
        //
        //     return variable.objectValue;
        // }


        // public int GetIntValue(VariableTypeTag typeTag)
        // {
        //     return (_variableFolder.GetVariable(typeTag) as VariableInt).CurrentValue;
        // }
        //
        // public float GetFloatValue(VariableTypeTag typeTag)
        // {
        //     return (_variableFolder.GetVariable(typeTag) as VariableFloat).CurrentValue;
        // }
        //
        // public bool GetBoolValue(VariableTypeTag typeTag)
        // {
        //     return (_variableFolder.GetVariable(typeTag) as VariableBool).CurrentValue;
        // }
        //往下找variable?
        //任何型別呢？

        public Dictionary<string, Func<IMonoDescriptable, object>> propertyCache = new();

        public Func<IMonoDescriptable, object> GetPropertyCache(string propertyName)
        {
            if (propertyCache.TryGetValue(propertyName, out var info))
                return info;

            var propertyInfo = GetType()
                .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

            // Debug.Log($"Property {propertyName} found in {sourceObject.GetType()}", sourceObject);

            if (propertyInfo == null)
            {
                propertyCache[propertyName] = null;
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

            Func<IMonoDescriptable, object> _getMyProperty = (source) =>
                getMethod.Invoke(source, null);
            propertyCache[propertyName] = _getMyProperty;
            return _getMyProperty;
        }

        public MonoEntityTag Key => DefaultTag;

        public MonoEntityTag[] GetKeys()
        {
            return DescriptableTags.ToArray();
        }

        public void EnterSceneAwake()
        {
            // _receiverTypeSet = new HashSet<GeneralEffectType>();
            // Debug.Log("EnterSceneAwake: " +name,this); //跑兩次？
            if (_receivers != null)
                foreach (var receiver in _receivers)
                {
                    // _receiverTypeSet.Add(receiver.EffectType);
                    if (!_receiverTypeMap.TryAdd(receiver._effectType, receiver))
                    {
                        Debug.Log("Receiver type already exists" + receiver._effectType, receiver);
                    }
                }

            foreach (var dealer in _dealers)
                if (_dealerTypeMap.TryAdd(dealer._effectType, dealer) == false)
                    Debug.LogWarning($"Dealer {dealer._effectType} already exists", dealer);

            // _dealerTypeMap = _dealers.ToDictionary(dealer => dealer.EffectType);

            // _dealerTypeSet = new HashSet<GeneralEffectType>();
            // if (_dealers != null)
            //     foreach (var dealer in _dealers)
            //     {
            //         // _dealerTypeSet.Add(dealer.EffectType);
            //         _dealerTypeMap[dealer.EffectType] = dealer;
            //     }
        }

        public IEnumerable<ValueDropdownItem<VariableTag>> GetVarTagOptions()
        {
            var tagDropdownItems = new List<ValueDropdownItem<VariableTag>>();
            foreach (var variable in VariableFolder.GetValues)
                tagDropdownItems.Add(
                    new ValueDropdownItem<VariableTag>(variable.name, variable._varTag)
                );
            return tagDropdownItems;
        }

        //繼承MonoDescriptable的class，可以透過這個方法來將所有的variable field mapping到VariableFolder
        private FieldInfo[] _variableFields;

        //撈出所有變數的tag塞到 DescriptableTags
        protected void FillVarTagsToMonoDescriptableTag()
        {
            if (VariableFolder == null)
                return;
            // if (DescriptableTags == null || DescriptableTagCount == 0)
            //     return;
            var variables = VariableFolder.GetValues;

            // 為所有 DescriptableTag 新增缺失的 variable tags
            // foreach (var descriptableTag in DescriptableTags)
            // {
            var descriptableTag = DefaultTag;
            if (descriptableTag == null)
                return;

            foreach (var variable in variables)
                if (!descriptableTag.containsVariableTypeTags.Contains(variable._varTag))
                    descriptableTag.containsVariableTypeTags.Add(variable._varTag);
#if UNITY_EDITOR
            EditorUtility.SetDirty(descriptableTag);
#endif
            // }
        }

        //FIXME: 好像不需要了？要繼承 MonoEntity 才需要
        [Button]
        private void FieldMapping()
        {
            //find all fields which inherit from AbstractMonoVariable
            //Check the value is not null
            //FIXME: 用名字mapping, 不好，直接用tag map, 沒有配到表示要生variable之類的

            _variableFields = GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(field => field.FieldType.IsSubclassOf(typeof(AbstractMonoVariable)))
                .ToArray();
            _variableFields.ForEach(field =>
            {
                //FIXME: 要加Type嗎...
                var fieldName = field.Name; //$"[{field.FieldType.Name}] {field.Name}";
                //把空白,_拿掉好了
                fieldName = fieldName.Replace(" ", "").Replace("_", "");
                //FIXME: 模糊搜尋？
                Debug.Log("fieldNameTarget: " + fieldName);
                var variable = VariableFolder.GetVariable(fieldName);
                if (variable != null)
                {
                    Debug.Log($"Set {fieldName} to {variable}", this);
                    field.SetValue(this, variable);
                }
                else
                {
                    Debug.Log("all variables count:" + VariableFolder.GetValues.Count);
                    VariableFolder.GetValues.ForEach(v =>
                        Debug.Log(v._varTag.GetStringKey, v._varTag)
                    );
                    Debug.LogError($"{fieldName} not found", this);
                }
                // var value = field.GetValue(this) as AbstractMonoVariable;
                // if (value == null)
                // {
                //     Debug.LogError($"Field {field.Name} is null", this);
                // }
            });
        }
    }
}

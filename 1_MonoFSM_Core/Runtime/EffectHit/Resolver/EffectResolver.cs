using System;
using _1_MonoFSM_Core.Runtime._1_States;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.Detection;
using MonoFSM.Foundation;
using MonoFSM.Variable.Attributes;
using MonoFSMCore.Runtime.LifeCycle;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MonoFSM.Runtime.Interact.EffectHit
{
    public abstract class EffectResolver
        : AbstractDescriptionBehaviour,
            IDefaultSerializable,
            IHitDataProvider,
            IResetStateRestore //, IHierarchyValueInfo,
    {
        [Required]
        [PreviewInInspector]
        [AutoParent]
        private MonoEntity _parentEntity;

        public T GetSchema<T>()
            where T : AbstractEntitySchema
        {
            return _parentEntity.GetSchema<T>();
        }

        public MonoEntity ParentEntity
        {
            get
            {
                this.EnsureComponentInParent(ref _parentEntity);
                return _parentEntity;
            }
        }

        [ShowInDebugMode]
        protected GeneralEffectHitData _currentHitData; //FIXME: 和last差在哪？

        [ShowInDebugMode]
        protected DetectData? _detectData;

#if UNITY_EDITOR
        [Header("Debug Info")]
        [ShowInDebugMode]
        protected IEffectHitData _lastHitData;
#endif

        public GeneralEffectHitData GetGeneralHitData()
        {
            return _currentHitData as GeneralEffectHitData;
        }

        public IEffectHitData GetHitData()
        {
            return _currentHitData;
        }

#if UNITY_EDITOR
        private GlobalObjectId _globalId;

        public GlobalObjectId GetGlobalId()
        {
            if (_globalId.targetObjectId == 0)
                _globalId = GlobalObjectId.GetGlobalObjectIdSlow(this);

            return _globalId;
        }
#endif

        // [Button]
        // private void Rename()
        // {
        //     name = "[" + TypeTag + "]" + _effectType.name.Replace("[EffectType]", "");
        // }

        public override string Description => FormatName(_effectType?.name) + _note; //要包含Detector的名字嗎？ 遠距離 的 player

        protected abstract string TypeTag { get; }

        [FormerlySerializedAs("EffectType")]
        [Required]
        [SOConfig("GeneralEffectType")]
        public GeneralEffectType _effectType; //改成private?

        public GeneralEffectType EffectType => _effectType;

        // public IEffectType getEffectType => EffectType;

        // [Required]
        [Component]
        [AutoChildren(DepthOneOnly = true)]
        [PreviewInInspector]
        protected EffectEnterNode _enterNode;

        [CompRef]
        [AutoChildren(DepthOneOnly = true)]
        protected EffectHitFailNode _failNode;

        public void OnEffectHitConditionFail(IEffectHitData data)
        {
            _failNode?.EventHandle(data);
        }

        [CompRef]
        [AutoChildren(DepthOneOnly = true)]
        protected EffectExitNode _exitNode;

        [CompRef]
        [AutoChildren(DepthOneOnly = true)]
        private AbstractConditionBehaviour[] _conditions =
            Array.Empty<AbstractConditionBehaviour>();

        [PreviewInInspector]
        public bool IsValid => isActiveAndEnabled && _conditions.IsAllValid(); //condition 可以burst?感覺不會比較快，這個數量級

        public IActor Owner => GetComponentInParent<IActor>();
        public string ValueInfo => IsValid ? "Valid" : "Invalid";
        public bool IsDrawingValueInfo => Application.isPlaying && isActiveAndEnabled;

        public void ResetStateRestore()
        {
            _currentHitData = null;
        }
    }
}

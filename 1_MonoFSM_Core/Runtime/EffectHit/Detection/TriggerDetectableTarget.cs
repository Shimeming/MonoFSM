using MonoFSM.Runtime.Interact.EffectHit;
using MonoFSM.Variable.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSM.Core.Detection
{
    [DisallowMultipleComponent]
    public class TriggerDetectableTarget
        : BaseEffectDetectTarget,
            IDetectableTarget,
            IColliderProvider
    {
        [Required]
        [CompRef]
        [AutoParent]
        private Collider _collider;
        //FIXME: 加的時候要同一層

        // [Component] [AutoChildren(DepthOneOnly = true)]
        // private GeneralEffectReceiver[] _effectReceivers;

        //FIXME: 要設成trigger
        // [Button("Add Collider")]
        // private void AddColliderComponent()
        // {
        //     if (_collider == null)
        //     {
        //         _collider = gameObject.AddComponent<BoxCollider>();
        //         var boxCollider = _collider as BoxCollider;
        //         if (boxCollider != null)
        //         {
        //             boxCollider.isTrigger = true;
        //         }
        //     }
        // }

        [Button("Setup as Trigger")]
        private void SetupAsTrigger()
        {
            if (_collider != null)
            {
                _collider.isTrigger = true;
            }
        }

        public GameObject TargetObject => gameObject;

        // public GeneralEffectReceiver[] EffectReceivers => _effectReceivers;
        public bool IsValidTarget => enabled && gameObject.activeInHierarchy && _collider != null;

        public Collider GetCollider() => _collider;

        private void Reset()
        {
            // AddColliderComponent();
        }
    }
}

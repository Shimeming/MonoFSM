using UnityEngine;

namespace MonoFSM.Runtime.Interact.EffectHit.Condition
{
    public class IsDealerOnReceiverCondition : AbstractConditionBehaviour
    {
        [SerializeField]
        private GeneralEffectReceiver _receiver;
        protected override bool IsValid => _receiver.HasDealerOverlap;
    }
}

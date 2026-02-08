using MonoFSM.Core.Runtime.Action;
using MonoFSM.Runtime.Interact.EffectHit;
using Sirenix.OdinInspector;

namespace _1_MonoFSM_Core.Runtime.EffectHit.Resolver
{
    public class ForceTriggerEffectAction : AbstractStateAction
    {
        bool IsEffectTypeNotMatched()
        {
            return _dealer?.EffectType != _receiver?.EffectType;
        }

        [Required]
        [InfoBox("Not Matched EffectType", InfoMessageType.Warning, nameof(IsEffectTypeNotMatched))]
        [DropDownRef]
        public GeneralEffectDealer _dealer;

        [Required] [DropDownRef] public GeneralEffectReceiver _receiver;

        protected override void OnActionExecuteImplement()
        {
            _receiver.ForceDirectEffectHit(_dealer, null);
        }
    }
}

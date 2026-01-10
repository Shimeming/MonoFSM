using MonoFSM.Runtime.Interact.EffectHit;

namespace _1_MonoFSM_Core.Runtime.EffectHit.Resolver
{
    public class EffectsFolder
    {
        [AutoChildren] GeneralEffectDealer[] _effectDealers;
        [AutoChildren] GeneralEffectReceiver[] _effectReceivers;
    }
}

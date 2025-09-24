using MonoFSM.Core;
using MonoFSM.Runtime.Variable;

namespace MonoFSM.Runtime.Interact.EffectHit
{
    // 用這個觸發action?
    public sealed class EffectEnterNode : AbstractEventHandler
    {
        //local variable, 這在這個enter下的生命週期
        [Component] //[Component?
        public VarEntity _hittingEntity; //to set
    }
}

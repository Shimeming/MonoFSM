using MonoFSM.Core.Module;
using MonoFSM.Variable;
using Sirenix.OdinInspector;

namespace MonoFSM.Runtime.Interact.EffectHit.Condition
{
    public class IsEnableHandleActive : AbstractConditionBehaviour
    {
        //tag mapping find...?
        // [DropDownRef]
        [ShowInInspector]
        public EnableHandle enableHandle => _enableHandleVar.Value as EnableHandle;

        public VarComp _enableHandleVar;

        //不夠好用，還是要用類別來mapping
        //用effect type可能也不夠耶, general tag?
        protected override bool IsValid => enableHandle?.isActiveAndEnabled ?? false;
        // _enableHandle != null && _enableHandle.isActiveAndEnabled;
    }
}

using MonoFSM.Core.Attributes;
using MonoFSM.Core.Runtime.Action;
using MonoFSMCore.Runtime.LifeCycle;
using Sirenix.OdinInspector;

namespace MonoFSM.Runtime.ObjectPool
{
    //FIXME: Despawn action?
    public class ReturnToPoolAction : AbstractStateAction
    {
        [Required] [PreviewInInspector] [AutoParent]
        private MonoObj _object;

        protected override void OnActionExecuteImplement()
        {
            _object.Despawn();
        }
    }
}
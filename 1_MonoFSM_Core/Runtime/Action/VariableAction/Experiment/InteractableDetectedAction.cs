using MonoFSM.Core.Runtime.Action;
using MonoFSM.Runtime.Variable;

namespace _1_MonoFSM_Core.Runtime.Action.VariableAction.Experiment
{
    public class InteractableDetectedAction : AbstractStateAction
    {
        public VarEntity _currentInteractable; //當前互動物件
        public HitDataEntityProvider _hitDataEntityProvider;

        protected override void OnActionExecuteImplement()
        {
            var receiverEntity = _hitDataEntityProvider.monoEntity;
            _currentInteractable.SetValue(receiverEntity, this);
        }
    }
}

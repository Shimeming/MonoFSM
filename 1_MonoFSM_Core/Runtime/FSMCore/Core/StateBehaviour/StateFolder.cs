using Fusion.Addons.FSM;
using MonoFSM.Core;
using MonoFSM.Runtime;

namespace _1_MonoFSM_Core.Runtime.FSMCore.Core.StateBehaviour
{
    public class StateFolder : MonoDictFolder<string, MonoStateBehaviour>
    {
        //parent folder?
        [AutoParent] MonoEntity _owningEntity;
        [Auto] private StateMachineLogic _context;
        public StateMachineLogic bindingContext => _owningEntity.StateFolder._context;
        protected override string DescriptionTag => "StateFolder";

        protected override void AddImplement(MonoStateBehaviour item)
        {
        }

        // protected override void AddExternalImplement(MonoStateBehaviour item)
        // {
        //     base.AddExternalImplement(item);
        //
        // }

        protected override void RemoveImplement(MonoStateBehaviour item)
        {
        }

        protected override bool CanBeAdded(MonoStateBehaviour item)
        {
            return true;
        }
    }
}

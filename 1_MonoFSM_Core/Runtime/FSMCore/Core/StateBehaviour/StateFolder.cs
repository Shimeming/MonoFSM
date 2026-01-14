using MonoFSM.Core;

namespace _1_MonoFSM_Core.Runtime.FSMCore.Core.StateBehaviour
{
    public class StateFolder : MonoDictFolder<string, MonoStateBehaviour>
    {
        protected override string DescriptionTag => "StateFolder";

        protected override void AddImplement(MonoStateBehaviour item)
        {
        }

        protected override void RemoveImplement(MonoStateBehaviour item)
        {
        }

        protected override bool CanBeAdded(MonoStateBehaviour item)
        {
            return true;
        }
    }
}

using MonoFSM.Variable.Attributes;
using UnityEngine;

namespace MonoFSM_Core.Runtime.StateBehaviour
{
    public class CanEnterNode : MonoBehaviour
    {
        [CompRef] [AutoChildren(DepthOneOnly = true)]
        private AbstractConditionBehaviour[] _conditions;

        public bool IsValid => _conditions.IsAllValid();
    }
}

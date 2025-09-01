using Fusion.Addons.FSM;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSM.Core
{
    public class IsStateCondition : AbstractConditionBehaviour
    {
        // [PreviewInInspector]
        // [AutoParent] StateMachineOwner _owner;
        [Required]
        [DropDownRef]
        [SerializeField]
        GeneralState _targetState;

        [Required]
        [DropDownRef]
        [SerializeField]
        private StateMachineLogic _fsmLogic;
        protected override bool IsValid => _fsmLogic.IsCurrentState(_targetState);

        //_owner.FsmContext.currentStateType == _targetState;
        public override string Description => $"Is {_targetState?.name}";
    }
}

using Fusion.Addons.FSM;
using UnityEngine;

namespace MonoFSM.Core
{
    public class IsStateCondition: AbstractConditionBehaviour
    {
        // [PreviewInInspector]
        // [AutoParent] StateMachineOwner _owner;
        [DropDownRef]
        [SerializeField]GeneralState _targetState;

        [SerializeField] private StateMachineLogic _fsmLogic;
        protected override bool IsValid => _fsmLogic.IsCurrentState(_targetState);

        //_owner.FsmContext.currentStateType == _targetState;
        public override string Description => $"{GetType().Name}({_targetState.name})";
    }
}
using MonoFSM_InputAction;
using MonoFSM.Core.Attributes;
using UnityEngine;

namespace RCGInputAction
{
    //FIXME:
    public class Player2DAxisActionCondition : AbstractConditionBehaviour
    {
        [DropDownRef] public MonoInputAction _inputAction;

        // [FormerlySerializedAs("ActionRef")] public InputActionReference _actionRef;
        protected override bool IsValid => _inputAction?.ReadValueVec2.magnitude > 0.01f;

        // [PreviewInInspector] [AutoParent] private PlayerInput playerInput;

        // private InputAction action =>
        //     playerInput != null && _actionRef != null ? playerInput.actions[_actionRef.action.name] : null;

        [PreviewInInspector]
        public Vector2 axisValue => _inputAction?.ReadValueVec2 ?? Vector2.zero;
    }
}

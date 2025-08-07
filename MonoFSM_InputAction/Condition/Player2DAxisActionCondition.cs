using MonoFSM.Core.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace RCGInputAction
{
    public class Player2DAxisActionCondition : AbstractConditionBehaviour
    {
        [FormerlySerializedAs("ActionRef")] public InputActionReference _actionRef;
        protected override bool IsValid => action is { inProgress: true };

        [PreviewInInspector] [AutoParent] private PlayerInput playerInput;
        
        private InputAction action =>
            playerInput != null && _actionRef != null ? playerInput.actions[_actionRef.action.name] : null;

        [PreviewInInspector]
        public Vector2 axisValue =>
            action?.ReadValue<Vector2>() ?? Vector2.zero; //ActionRef.action.ReadValue<Vector2>();
    }
}
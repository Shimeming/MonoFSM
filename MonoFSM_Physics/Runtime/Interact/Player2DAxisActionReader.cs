using MonoFSM.Core.Attributes;
using MonoFSM.Core.Runtime.Action;
using MonoFSM.Variable;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace MonoFSM_Physics.Runtime
{
    public class Player2DAxisActionReader : AbstractStateAction
    {
        [DropDownRef]
        [SerializeField] VarVector2 _axisValueProvider;

        [FormerlySerializedAs("ActionRef")] public InputActionReference _actionRef;

        [PreviewInInspector][AutoParent] private PlayerInput playerInput;

        private InputAction action =>
            playerInput != null && _actionRef != null ? playerInput.actions[_actionRef.action.name] : null;

        [PreviewInInspector]
        public Vector2 axisValue =>
            action?.ReadValue<Vector2>() ?? Vector2.zero;

        protected override void OnActionExecuteImplement()
        {
            if (action == null) return;
            _axisValueProvider.SetValue(axisValue, this);
        }
    }
}

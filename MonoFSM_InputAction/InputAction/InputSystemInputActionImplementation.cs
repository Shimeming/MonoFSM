using MonoFSM.Core.Attributes;
using MonoFSM.Foundation;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MonoFSM_InputAction
{
    //FIXME: 應該綁這個為主？DI IsPressed實作
    [RequireComponent(typeof(MonoInputAction))]
    public class InputSystemInputActionImplementation : AbstractDescriptionBehaviour, IInputActionImplementation
    {

        [Required]
        [PreviewInInspector] [AutoParent] private PlayerInput _localPlayerInput;
        public int InputActionId => _inputActionData.actionID; //還是monobehaviour自己assign就好？

        [InlineEditor]
        [SOConfig("PlayerInputActionData")] [SerializeField]
        protected InputActionData _inputActionData;

        // private bool _readLocalVec2;

        // private InputActionMap _inputActionMap;
        public InputAction myAction =>
            _inputActionData ? _localPlayerInput?.actions[_inputActionData?.inputAction?.name] : null;
        // public InputAction myAction => _localPlayerInput.currentActionMap.FindAction(_inputActionData.inputAction.name);

        bool IInputActionImplementation.IsLocalPressed =>
            Application.isPlaying && (myAction.IsPressed() || myAction.WasPressedThisFrame());

        [ShowInDebugMode]
        Vector2 IInputActionImplementation.ReadLocalVec2 => myAction.ReadValue<Vector2>();
        Vector2 IInputActionImplementation.Vec2Value => ((IInputActionImplementation)this).ReadLocalVec2;
        [ShowInInspector]
        bool IInputActionImplementation.IsVec2 => _inputActionData.inputAction.action.expectedControlType == "Vector2";

        [ShowInDebugMode]
        bool IInputActionImplementation.IsPressed => myAction?.IsPressed() ?? false; //如果外掛
        bool IInputActionImplementation.WasPressed => myAction.WasPressedThisFrame();
        bool IInputActionImplementation.WasReleased => myAction.WasReleasedThisFrame();

        protected override string DescriptionTag => "Input";
        public override string Description => _inputActionData.name;
    }
}

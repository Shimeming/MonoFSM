using MonoFSM.Core.Attributes;
using MonoFSM.Core.Simulate;
using MonoFSM.Foundation;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MonoFSM_InputAction
{
    //FIXME: 應該綁這個為主？DI IsPressed實作
    [RequireComponent(typeof(MonoInputAction))]
    public class InputSystemInputActionImplementation
        : AbstractDescriptionBehaviour,
            IUpdateSimulate,
            IInputActionImplementation
    {
        // [Required]
        // [PreviewInInspector] [AutoParent] private PlayerInput _localPlayerInput;
        public int InputActionId => _inputActionData.actionID; //還是monobehaviour自己assign就好？

        [InlineEditor]
        [SOConfig("PlayerInputActionData")]
        [SerializeField]
        protected InputActionData _inputActionData;

        // 時間追蹤欄位
        [ShowInInspector]
        private float _pressStartTime = -1f;

        [ShowInInspector]
        private float _lastPressedTime = -1f;
        private bool _wasPressedLastFrame;

        // private bool _readLocalVec2;

        // private InputActionMap _inputActionMap;
        public InputAction myAction => _inputActionData._inputAction.action;

        // public InputAction myAction =>
        // _inputActionData && _inputActionData._inputAction != null
        //     ? _localPlayerInput?.actions?[_inputActionData?._inputAction?.name] //好像不要用了？
        //     : null;
        // public InputAction myAction => _localPlayerInput.currentActionMap.FindAction(_inputActionData.inputAction.name);

        bool IInputActionImplementation.IsLocalPressed =>
            Application.isPlaying && (myAction.IsPressed() || myAction.WasPressedThisFrame());

        [ShowInDebugMode]
        Vector2 IInputActionImplementation.ReadLocalVec2 =>
            myAction?.ReadValue<Vector2>() ?? Vector2.zero;
        Vector2 IInputActionImplementation.Vec2Value =>
            ((IInputActionImplementation)this).ReadLocalVec2;

        [ShowInInspector]
        bool IInputActionImplementation.IsVec2 =>
            _inputActionData?._inputAction?.action?.expectedControlType == "Vector2";

        [ShowInDebugMode]
        bool IInputActionImplementation.IsPressed => myAction?.IsPressed() ?? false;
        bool IInputActionImplementation.WasPressed => myAction.WasPressedThisFrame();
        bool IInputActionImplementation.WasReleased => myAction.WasReleasedThisFrame();

        [ShowInDebugMode]
        float IInputActionImplementation.PressTime
        {
            get
            {
                if (!Application.isPlaying || _pressStartTime < 0f)
                    return 0f;

                bool isCurrentlyPressed = myAction?.IsPressed() ?? false;
                if (isCurrentlyPressed)
                    return ((IInputActionImplementation)this).GetCurrentTime() - _pressStartTime;

                return 0f;
            }
        }

        [ShowInDebugMode]
        float IInputActionImplementation.LastPressedTime => _lastPressedTime;

        /// <summary>
        /// 獲取當前時間 - 預設使用 Time.time，可在子類別中 override 使用 Runner.SimulationTime
        /// </summary>
        float IInputActionImplementation.GetCurrentTime() => WorldUpdateSimulator.SimulationTime;

        protected override string DescriptionTag => "Input";
        public override string Description => _inputActionData?.name;

        public void Simulate(float deltaTime)
        {
            // if (!Application.isPlaying || myAction == null)
            //     return;

            bool isCurrentlyPressed = myAction.IsPressed();
            float currentTime = ((IInputActionImplementation)this).GetCurrentTime();

            // 檢測按下事件
            if (isCurrentlyPressed && !_wasPressedLastFrame)
            {
                _pressStartTime = currentTime;
                _lastPressedTime = currentTime;
            }
            // 檢測放開事件
            else if (!isCurrentlyPressed && _wasPressedLastFrame)
            {
                _pressStartTime = -1f;
            }

            _wasPressedLastFrame = isCurrentlyPressed;
        }

        public void AfterUpdate()
        {
            // throw new System.NotImplementedException();
        }
    }
}

using MonoFSM_InputAction;

namespace Fusion.Addons.KCC.ECM2.Examples.Networking.Fusion_v2.Characters.Scripts.Input
{
    //FIXME: move不能用這個
    public class InputActionWasPressedCondition : AbstractConditionBehaviour
    {
        public override string Description => $"{_inputAction.name} {_inputActionType}";

        public enum InputActionType
        {
            WasPressed,
            IsPressed,
            WasReleased,
        }

        public InputActionType _inputActionType = InputActionType.WasPressed;

        //valid的timing怎麼處理.. networkcondition, 太難了ㄅ 只看state?
        protected override bool IsValid
        {
            get
            {
                switch (_inputActionType)
                {
                    case InputActionType.WasPressed:
                        this.Log(
                            "InputActionWasPressedCondition IsValid: ",
                            _inputAction.WasPressed
                        );
                        return _inputAction.WasPressed;
                    case InputActionType.IsPressed:
                        this.Log(
                            "InputActionWasPressedCondition IsValid: ",
                            _inputAction.IsPressed
                        );
                        return _inputAction.IsPressed;
                    case InputActionType.WasReleased:
                        this.Log(
                            "InputActionWasPressedCondition IsValid: ",
                            _inputAction.WasReleased
                        );
                        return _inputAction.WasReleased;
                    default:
                        return false;
                }
            }
        }

        // _playerInputProvider.GetPlayerInput().WasPressed(actionData.actionID); //isvalid的timing也要小心

        // public InputActionData actionData;

        //FIXME: 用一個介面
        [DropDownRef]
        public MonoInputAction _inputAction;
        //resolve 去哪找？往上找
        // [AutoParent] AbstractFusionPlayerInput playerInput;
        // [AutoParent] private IPlayerInputProvider _playerInputProvider;
    }
}

using _1_MonoFSM_Core.Runtime._3_FlagData;
using MonoFSM.Core.Attributes;
using MonoFSM.InputAction;
using RCGInputAction;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[CreateAssetMenu(
    menuName = "MonoFSM/Input/InputActionData",
    fileName = "InputActionData",
    order = 0
)]
public class InputActionData : AbstractSOConfig
{
    [ShowInInspector]
    private InputActionDataCollection myCollection => InputActionDataCollection.Instance;

    [FormerlySerializedAs("inputAction")]
    [Required]
    public InputActionReference _inputAction;

    [ShowInInspector]
    private string expectedControlType => _inputAction?.action?.expectedControlType;

    //FIXME://enum mapping for network, 改成自動mapping
    [PreviewInInspector]
    public int actionID;

    //local 多人是錯的
    public bool WasPressed()
    {
        return _inputAction.action.WasPressedThisFrame();
    }

    public bool IsPressed()
    {
        return _inputAction.action.IsPressed();
    }

    public bool WasReleased()
    {
        return _inputAction.action.WasReleasedThisFrame();
    }

    public InputAction GetAction(PlayerInput playerInput)
    {
        if (_inputAction == null)
            return null;
        return playerInput.GetAction(_inputAction);
    }
}

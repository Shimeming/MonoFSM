using _1_MonoFSM_Core.Runtime._3_FlagData;
using MonoFSM.Core.Attributes;
using MonoFSM.InputAction;
using RCGInputAction;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "MonoFSM/Input/InputActionData", fileName = "InputActionData", order = 0)]
public class InputActionData : MonoSOConfig
{
    [ShowInInspector] private InputActionDataCollection myCollection => InputActionDataCollection.Instance;
    [Required]
    public InputActionReference inputAction;

    //FIXME://enum mapping for network, 改成自動mapping
    [PreviewInInspector]
    public int actionID;
    //local 多人是錯的
    public bool WasPressed() => inputAction.action.WasPressedThisFrame();
    public bool IsPressed() => inputAction.action.IsPressed();
    public bool WasReleased() => inputAction.action.WasReleasedThisFrame();



    public InputAction GetAction(PlayerInput playerInput)
    {
        if (inputAction == null)
            return null;
        return playerInput.GetAction(inputAction);
    }
}



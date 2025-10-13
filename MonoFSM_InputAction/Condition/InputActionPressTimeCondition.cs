using MonoFSM.Core.Attributes;
using MonoFSM.Variable;
using UnityEngine;

namespace MonoFSM_InputAction.Condition
{
    public class InputActionPressTimeCondition : AbstractConditionBehaviour
    {
        [DropDownRef]
        public MonoInputAction _inputAction;

        [InlineField]
        [SerializeField]
        VarFloatFoldOut _pressDuration = new VarFloatFoldOut();

        protected override bool IsValid =>
            _pressDuration.Value > 0 && _inputAction.PressTime >= _pressDuration.Value;

        public override string Description =>
            _inputAction != null
                ? $"{_inputAction.name} Pressed for {_pressDuration.Value} seconds"
                : "No Input Action";
    }
}

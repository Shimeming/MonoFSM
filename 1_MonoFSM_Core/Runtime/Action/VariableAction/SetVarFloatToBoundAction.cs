using System;
using MonoFSM.Variable;

namespace MonoFSM.Core.Runtime.Action.VariableAction
{
    public class SetVarFloatToBoundAction : AbstractStateAction
    {
        public override string Description => "Set $" + _targetVar?.name + " to " +
                                              _boundType;

        public enum BoundType
        {
            Min,
            Max
        }

        [DropDownRef] public VarFloat _targetVar;
        public BoundType _boundType = BoundType.Max;

        protected override void OnActionExecuteImplement()
        {
            var value = _boundType == BoundType.Min ? _targetVar.Min : _targetVar.Max;
            _targetVar.SetValue(value, this);
        }
    }
}

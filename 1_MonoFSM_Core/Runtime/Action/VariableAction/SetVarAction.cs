using MonoFSM.Core.DataProvider;
using MonoFSM.Core.Runtime.Action;
using MonoFSM.Runtime.Attributes;
using MonoFSM.Variable;
using UnityEngine;

namespace _1_MonoFSM_Core.Runtime.Action.VariableAction
{
    public class SetVarAction : AbstractStateAction
    {
        //FIXME: 怎麼Validate 他有 Variable?
        [ValueTypeValidate(IsVariableNeeded = true)] [SerializeField] [DropDownRef]
        //[ValueTypeValidate(typeof(AbstractMonoVariable))]
        private ValueProvider _targetVarProvider;

        [SerializeField] [DropDownRef] private ValueProvider _sourceValueProvider;

        protected override void OnActionExecuteImplement()
        {
            //typeCast問題？
            _targetVarProvider.GetVar<AbstractMonoVariable>()?.SetValueByValueProvider(_sourceValueProvider, this);
        }

        public override string Description
        {
            get
            {
                if (_targetVarProvider == null)
                    return "SetVarAction: No Target Variable";
                if (_sourceValueProvider == null)
                    return "SetVarAction: No Source Value Provider";

                return $"Set {_targetVarProvider.Description} = {_sourceValueProvider.Description}";
            }
        }
    }
}
using System;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.DataProvider;
using MonoFSM.Foundation;
using MonoFSM.Variable;
using MonoFSM.Variable.Attributes;

namespace MonoFSM.VarRefOld
{
    public class TargetVarRef : AbstractDescriptionBehaviour, IVariableProvider
    {
        //Assign對象，必定是variable provider
        [CompRef] [Auto] private AbstractVariableProviderRef _providerRef;

        public AbstractMonoVariable VarRaw => _providerRef?.VarRaw;
        public bool IsVariableValid => _providerRef?.IsVariableValid ?? false;
        public Type VariableType => _providerRef?.VariableType;
        public Type GetValueType => _providerRef?.ValueType;

        public TVariable GetVar<TVariable>() where TVariable : AbstractMonoVariable
        {
            return _providerRef.GetVar<TVariable>();
        }

        [PreviewInInspector]
        public override string Description
        {
            get
            {
                _providerRef = GetComponent<AbstractVariableProviderRef>();
                if (_providerRef == null) return "null providerRef";
                return _providerRef.Description;
            }
        }
        protected override string DescriptionTag => "Target Var";


        public override string ToString()
        {
            return Description;
        }
    }
}
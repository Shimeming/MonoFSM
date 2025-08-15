using System;
using MonoFSM.Core.Attributes;

namespace MonoFSM.Variable
{
    [Serializable]
    public class VarFloatWrapper : VarWrapper<VarFloat, float>
    {
    }

    [Serializable]
    public class VarWrapper<TVar, TValue> where TVar : AbstractMonoVariable
    {
        [SOConfig("VariableType")] public VariableTag _bindTag;
        public TVar _var;
        public TValue Value => _var.Get<TValue>();
    }
}

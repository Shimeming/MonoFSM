using System;
using MonoFSM.Core.Attributes;
using Object = UnityEngine.Object;

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

        public TValue Value
        {
            get => _var.Get<TValue>(); //無窮回全了...
            set => _var.SetValue(value, null); //FIXME: 不好debug? wrapper要拿得到 parent object?
        }

        public void SetValue(TValue value, Object byWho)
        {
            _var.SetValue(value, byWho);
        }

    }
}

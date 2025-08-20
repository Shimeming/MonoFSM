using System;
using System.Collections.Generic;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.Variable;
using MonoFSM.Runtime;
using MonoFSM.Runtime.Variable;
using Object = UnityEngine.Object;

namespace MonoFSM.Variable
{
    [Serializable]
    public class VarBoolWrapper : VarWrapper<VarBool, bool> { }

    [Serializable]
    public class VarFloatWrapper : VarWrapper<VarFloat, float> { }

    [Serializable]
    public class VarEntityWrapper : VarWrapper<VarEntity, MonoEntity> { }

    [Serializable]
    public class VarListEntityWrapper : VarWrapper<VarListEntity, List<MonoEntity>> { }

    [Serializable]
    public class VarWrapper<TVar, TValue>
        where TVar : AbstractMonoVariable
    {
        [SOConfig("VariableType")]
        public VariableTag _bindTag;
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

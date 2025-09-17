using System;
using System.Collections.Generic;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.Variable;
using MonoFSM.Runtime;
using MonoFSM.Runtime.Variable;
using MonoFSM.Runtime.Vote;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MonoFSM.Variable
{
    [Serializable]
    public class VarVector3Wrapper : VarWrapper<VarVector3, Vector3> { }

    [Serializable]
    public class VarVoteWrapper : VarWrapper<VarVote, bool> { }

    [Serializable]
    public class VarBoolWrapper : VarWrapper<VarBool, bool> { }

    [Serializable]
    public class VarGameDataWrapper : VarWrapper<VarBool, bool> { }

    [Serializable]
    public class VarFloatWrapper : VarWrapper<VarFloat, float> { }

    [Serializable]
    public class VarEntityWrapper : VarWrapper<VarEntity, MonoEntity> { }

    [Serializable]
    public class VarListEntityWrapper : VarWrapper<VarListEntity, List<MonoEntity>> { }

    public abstract class AbstractVarWrapper { }

    //FIXME: 真的有需要wrapper嗎？
    [Serializable]
    public class VarWrapper<TVar, TValue> : AbstractVarWrapper
        where TVar : AbstractMonoVariable
    {
        [SOConfig("VariableType")]
        [Required]
        public VariableTag _bindTag;

        [Required]
        public TVar _var;

        public TValue Value
        {
            get { return _var.Get<TValue>(); }
            set => _var.SetRaw(value, _var); //FIXME: 不好debug? wrapper要拿得到 parent object?
        }

        public void SetValue(TValue value, Object byWho)
        {
            _var.SetRaw(value, byWho);
        }
    }
}

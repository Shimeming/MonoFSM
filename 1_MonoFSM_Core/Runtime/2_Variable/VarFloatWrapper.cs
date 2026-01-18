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
    public class VarVector3Wrapper : VarWrapper<VarVector3, Vector3>
    {
        public VarVector3Wrapper() { }

        public VarVector3Wrapper(Vector3 defaultValue)
            : base(defaultValue) { }
    }

    [Serializable]
    public class VarVoteWrapper : VarWrapper<VarVote, bool>
    {
        public VarVoteWrapper() { }

        public VarVoteWrapper(bool defaultValue)
            : base(defaultValue) { }
    }

    [Serializable]
    public class VarBoolWrapper : VarWrapper<VarBool, bool>
    {
        public VarBoolWrapper() { }

        public VarBoolWrapper(bool defaultValue)
            : base(defaultValue) { }
    }

    [Serializable]
    public class VarGameDataWrapper : VarWrapper<VarGameData, GameData>
    {
        public VarGameDataWrapper() { }

        public VarGameDataWrapper(GameData defaultValue)
            : base(defaultValue) { }
    }

    [Serializable]
    public class VarFloatWrapper : VarWrapper<VarFloat, float>
    {
        public VarFloatWrapper() { }

        public VarFloatWrapper(float defaultValue)
            : base(defaultValue) { }
    }

    [Serializable]
    public class VarEntityWrapper : VarWrapper<VarEntity, MonoEntity>
    {
        public VarEntityWrapper() { }

        public VarEntityWrapper(MonoEntity defaultValue)
            : base(defaultValue) { }
    }

    [Serializable]
    public class VarListEntityWrapper : VarWrapper<VarListEntity, List<MonoEntity>>
    {
        public VarListEntityWrapper() { }

        public VarListEntityWrapper(List<MonoEntity> defaultValue)
            : base(defaultValue) { }
    }

    public abstract class AbstractVarWrapper { }

    //FIXME: 真的有需要wrapper嗎？
    [Serializable]
    public class VarWrapper<TVar, TValue> : AbstractVarWrapper
        where TVar : AbstractMonoVariable
    {
        [BoxGroup("Var")]
        [HideIf("_var", null, false)]
        [ShowInInspector]
        [SerializeField]
        private TValue _tempValue;

        [BoxGroup("Var")]
        [SOConfig("VariableType")]
        [Required]
        public VariableTag _bindTag;

        [BoxGroup("Var")]
        [Required]
        public TVar _var;

        public VarWrapper() { }

        public VarWrapper(TValue defaultValue)
        {
            _tempValue = defaultValue;
        }

        public TValue Value
        {
            get
            {
                if (_var == null)
                    return _tempValue;
                return _var.Get<TValue>();
            }
            set
            {
                if (_var == null)
                {
                    _tempValue = value;
                    return;
                }

                _var.SetRaw(value, _var); //FIXME: 不好debug? wrapper要拿得到 parent object?
            }
        }

        public void SetValue(TValue value, Object byWho)
        {
            if (_var == null)
            {
                _tempValue = value;
                return;
            }
            _var.SetRaw(value, byWho);
        }
    }
}

using System.Collections.Generic;
using _1_MonoFSM_Core.Runtime.Action.AnimatorActions;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.DataProvider;
using MonoFSM.Core.Runtime.Action;
using MonoFSM.Variable.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

// using MonoFSM.Core.Runtime.Conditions;

namespace MonoFSM.Animation
{
    /// <summary>
    ///     實驗性，還要整理
    /// </summary>
    public class AnimatorParameterSetValueAction : AbstractStateAction
    {
        public enum ValueType
        {
            Bool,
            Float,
            Int,
        }

        public override string Description =>
            $"Set Animator Parameter {ParameterName} to {valueType} Value";

        public ValueType valueType;
        public bool IsUpdateSet = false;

        [HideIf(nameof(_animatorRefProvider))]
        [TitleGroup("Animator")]
        [BoxGroup("Animator/Animator")]
        [Required]
        [DropDownRef]
        [FormerlySerializedAs("animator")]
        public Animator _animator;

        [ShowInInspector]
        private Animator animator =>
            _animatorRefProvider != null ? _animatorRefProvider.Value : _animator;

        [TitleGroup("Animator")]
        [BoxGroup("Animator/Animator")]
        [SerializeField]
        [CompRef]
        [AutoChildren(DepthOneOnly = true)]
        private AnimatorRefProvider _animatorRefProvider;

        [ValueDropdown(nameof(GetParameterNames))]
        public string ParameterName;

        //每種datatype拆開？
        [SerializeField]
        private float _interpolate;

        //invert bool value?
        // public bool _boolValue;

        #region Float

        private float _lastValue;

        public float _floatValue;

        #endregion

        public int _intValue;

        [Auto]
        [PreviewInInspector]
        public IFloatProvider _floatValueSource;

        // [DropDownRef]
        // public AbstractVariable sourceVariable;
        private IEnumerable<string> GetParameterNames()
        {
            var parameters = animator.parameters;
            foreach (var parameter in parameters)
                yield return parameter.name;
        }

        private void SetValue()
        {
            if (_floatValueSource != null)
            {
                if (_interpolate == 0)
                    _lastValue = _floatValueSource.Value;
                else
                    _lastValue = Mathf.MoveTowards(
                        _lastValue,
                        _floatValueSource.Value,
                        _interpolate * bindingState.DeltaTime
                    ); //FIXME: 不一定會有bindingState? 還是乾脆拿logic的就好了？

                animator.SetFloat(ParameterName, _lastValue);
            }
            else
            {
                switch (valueType)
                {
                    case ValueType.Bool:
                        animator.SetBool(ParameterName, IsValid); //直接對著valid做不是很爽？ FIXME: 要拆出去？
                        break;
                    case ValueType.Float:
                        animator.SetFloat(ParameterName, _floatValue);
                        break;
                    case ValueType.Int:
                        animator.SetInteger(ParameterName, _intValue);
                        break;
                }
            }
        }

        protected override void OnActionExecuteImplement()
        {
            SetValue();
        }

        protected override bool ForceExecuteInValid => true;

        //FIXME: 拔掉！
        // protected override void OnStateUpdateImplement()
        // {
        //     if (IsUpdateSet) SetValue();
        // }
    }
}

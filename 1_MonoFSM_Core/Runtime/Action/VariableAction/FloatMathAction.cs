using MonoFSM.Core.Attributes;
using MonoFSM.Core.Runtime._0_Pattern.DataProvider.ComponentWrapper;
using MonoFSM.Core.Runtime.Action;
using MonoFSM.Variable;
using MonoFSM.Variable.Attributes;
using MonoFSM.VarRefOld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSM.Runtime.Interact.EffectHit.Resolver.ApplyEffect
{
    // //最完整的應該用這個
    public class FloatMathAction : AbstractStateAction
    {
        public VarFloat _targetVar;
        public VarFloat _source1Var;

        // [AutoChildren] [CompRef] private TargetVarRef _targetVariableProvider; // }
        //
        // [AutoChildren] [CompRef] private SourceValueRef _source1VariableProvider;

        [ShowIf(nameof(IsSource2Needed))]
        public VarFloat _source2Var;

        // [AutoChildren] [CompRef]
        // private SourceValue2Ref _source2VariableProvider;



        public ArithmeticType Arithmetic;

        // private bool IsSource2NotNeeded()
        // {
        //     return Arithmetic == ArithmeticType.AdditionAssign || Arithmetic == ArithmeticType.SubtractionAssign;
        // }

        private bool IsSource2Needed()
        {
            if (_source1Var == null)
                return false;
            return Arithmetic != ArithmeticType.AdditionAssign
                && Arithmetic != ArithmeticType.SubtractionAssign;
        }

        // public OperandType _setter;
        // public OperandType _operator1;
        //
        // public OperandType _operator2;

        // private VariableTag op1 => _operator1 == OperandType.Dealer ? dealerVariableProvider?._varTag : receiverVariableProvider?._varTag;

        // private VariableTag op2 =>
        //     _operator2 == OperandType.Dealer ? dealerVariableProvider?._varTag : receiverVariableProvider?._varTag;

        // private AbstractMonoVariable setterVariable => _targetVar?.VarRaw;
        // _setter == OperandType.Dealer
        //     ? dealerVariableProvider?.GetVarRaw()
        //     : receiverVariableProvider?.GetVarRaw();

        private string ArithmeticString =>
            Arithmetic switch
            {
                ArithmeticType.Add => "+",
                ArithmeticType.Subtract => "-",
                ArithmeticType.Multiply => "*",
                ArithmeticType.Divide => "/",
                ArithmeticType.Modulo => "%",
                _ => "+",
            };

        [PreviewInInspector]
        public override string Description
        {
            get
            {
                var targetDesc = _targetVar != null ? _targetVar.Description : "null";
                var source1Desc = _source1Var != null ? _source1Var.Description : "null";

                return Arithmetic switch
                {
                    ArithmeticType.AdditionAssign => $"{targetDesc} += {source1Desc}",
                    ArithmeticType.SubtractionAssign => $"{targetDesc} -= {source1Desc}",
                    _ =>
                        $"{targetDesc} = {source1Desc} {ArithmeticString} {_source2Var?.Description}",
                };
            }
        }

        // $"{setterVariable?.name} = {_operator1}.{op1?.name} {ArithmeticString} {_operator2}.{op2?.name}";
        //要用entry?


        // [DropDownRef] public VariableFloat dealerVariable;


        //FIXME: target Variable會交換...有時候想處理的是Dealer，有時候想處理的是Receiver

        public enum ArithmeticType
        {
            Add,
            Subtract,
            Multiply,
            Divide,
            Modulo,
            AdditionAssign,
            SubtractionAssign,
            //+=,-=?
        }

        // protected override void ApplyEffect(GeneralEffectDealer dealer, GeneralEffectReceiver receiver)
        protected override void OnActionExecuteImplement()
        {
            if (_targetVar == null || _source1Var == null)
            {
                Debug.LogError(
                    "EffectHitFloatArithmeticAction: Target or Source1 variable provider is not set.",
                    this
                );
                return;
            }

            var targetValue = _targetVar.Value;
            var value1 = _source1Var.Value;
            float result = 0;
            if (Arithmetic == ArithmeticType.AdditionAssign)
                result = targetValue + value1;
            else if (Arithmetic == ArithmeticType.SubtractionAssign)
                result = targetValue - value1;
            else
            {
                var value2 = _source2Var.Value;
                result = Calculate(value1, value2);
            }

            _targetVar.SetValue(result, this);
            // var dealerValue = dealerVariableProvider.GetValueFrom(dealer);
            // var receiverValue = receiverVariableProvider.GetValueFrom(receiver);
            // Debug.Log(
            //     $"{_setter} = {dealerVariableProvider._varTag.name} dealerValue: {dealerValue}, {Arithmetic} {receiverVariableProvider._varTag.name} receiverValue: {receiverValue}",
            //     this);
            // var value1 = _operator1 == OperandType.Dealer ? dealerValue : receiverValue;
            // var value2 = _operator2 == OperandType.Dealer ? dealerValue : receiverValue;
            // if (_setter == OperandType.Dealer)
            //     dealerVariableProvider.SetValue(
            //         Calculate(value1, value2), this);
            // else
            //     receiverVariableProvider.SetValue(
            //         Calculate(value1, value2), this);
        }

        private float Calculate(float source1, float source2)
        {
            return Arithmetic switch
            {
                ArithmeticType.Add => source1 + source2,
                ArithmeticType.Subtract => source1 - source2,
                ArithmeticType.Multiply => source1 * source2,
                ArithmeticType.Divide => source1 / source2,
                ArithmeticType.Modulo => source1 % source2,

                _ => source1,
            };
        }
    }
}

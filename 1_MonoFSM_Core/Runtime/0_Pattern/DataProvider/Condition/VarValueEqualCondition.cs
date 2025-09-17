using System;
using MonoFSM.Core.Attributes;
using MonoFSM.Variable;
using MonoFSM.VarRefOld;
using UnityEngine;

namespace MonoFSM.Core.DataProvider.Condition
{
    //ex: FloatCompareCondition
    [Obsolete]
    public class VarValueEqualCondition : AbstractConditionBehaviour //
    {
        //可能想要比Value vs Vlue,
        // [Component][PreviewInInspector] IVariableProvider _sourceVariableProvider;
        // [Component][PreviewInInspector] IVariableProvider _targetVariableProvider;
        [AutoChildren]
        [Component]
        [PreviewInInspector]
        private TargetVarRef _targetVarRef;

        [AutoChildren]
        [Component]
        [PreviewInInspector]
        private SourceValueRef _sourceValueRef;

        private AbstractMonoVariable targetVariable => _targetVarRef.VarRaw;

        // AbstractMonoVariable sourceVariable => _sourceValueRef?.VarRaw;

        protected override bool IsValid => throw new NotImplementedException(); //targetVariable.objectValue == _sourceValueRef.objectValue; //這感覺不對啊？

        public override string Description => $"{_sourceValueRef} == {_targetVarRef}";
        // targetVariable?.objectValue != null &&
        //                                   _sourceValueRef?.GetValue() ==
        //                                 targetVariable?.objectValue;
    }
}

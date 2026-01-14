using System;
using MonoFSM.Core.DataProvider;
using MonoFSM.Core.Runtime.Action;
using MonoFSM.Runtime.Attributes;
using MonoFSM.Variable;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _1_MonoFSM_Core.Runtime.Action.VariableAction
{
    [Obsolete]
    public class SetVarAction : AbstractStateAction
    {
        //FIXME: 必定會有人null
        // [HideIf(nameof(_targetVarProvider))]
        // [DropDownRef]
        // [SerializeField] AbstractMonoVariable _localVar; //2. local var

        // [HideIf(nameof(_localVar))]
        [ValueTypeValidate(IsVariableNeeded = true)]
        [SerializeField]
        [DropDownRef]
        private ValueProvider _targetVarProvider; //1. 遠方的var

        [InfoBox("$TypeValidationMessage", InfoMessageType.Warning, VisibleIf = "ShowTypeWarning")]
        [InfoBox("$TypeValidationMessage", InfoMessageType.None, VisibleIf = "ShowTypeInfo")]
        // [PropertyOrder(100)]
        [SerializeField]
        [DropDownRef]
        private ValueProvider _sourceValueProvider;

        private string TypeValidationMessage
        {
            get
            {
                //FIXME: 不一定拿得到var啊
                var targetVar = GetTargetVariable();
                if (targetVar == null || _sourceValueProvider == null)
                    return "請設定目標變數和來源值提供者";

                var targetType = targetVar.ValueType;
                var sourceType = _sourceValueProvider.ValueType;

                if (targetType == null || sourceType == null)
                    return "無法取得型別資訊";

                if (ValidateValueType(targetVar, _sourceValueProvider))
                    return $"型別相符：{targetType.Name} ← {sourceType.Name}";
                else
                    return $"型別不相符：{targetType.Name} ← {sourceType.Name}";
            }
        }

        private bool ShowTypeWarning =>
            !ShowTypeInfo && GetTargetVariable() != null && _sourceValueProvider != null;

        private bool ShowTypeInfo =>
            GetTargetVariable() != null
            && _sourceValueProvider != null
            && ValidateValueType(GetTargetVariable(), _sourceValueProvider);

        //FIXME:
        private AbstractMonoVariable GetTargetVariable()
        {
            if (!Application.isPlaying)
                return null;
            // if (_localVar != null)
            //     return _localVar;
            return _targetVarProvider?.GetVar<AbstractMonoVariable>();
        }

        protected override void OnActionExecuteImplement()
        {
            var targetVar = GetTargetVariable();
            if (targetVar == null)
            {
                Debug.LogError("SetVarAction: Target variable not found", this);
                return;
            }

            if (_sourceValueProvider == null)
            {
                Debug.LogError("SetVarAction: Source value provider not found", this);
                return;
            }

            if (!ValidateValueType(targetVar, _sourceValueProvider))
            {
                Debug.LogError(
                    $"SetVarAction: Type mismatch - Variable type: {targetVar.ValueType}, Provider type: {_sourceValueProvider.ValueType}",
                    this
                );
                return;
            }

            throw new NotImplementedException();
            // targetVar.SetValueByValueProvider(_sourceValueProvider, this);
        }

        private bool ValidateValueType(AbstractMonoVariable targetVar, ValueProvider sourceProvider)
        {
            var targetType = targetVar.ValueType;
            var sourceType = sourceProvider.ValueType;

            if (targetType == null || sourceType == null)
                return false;

            return targetType.IsAssignableFrom(sourceType)
                || sourceType.IsAssignableFrom(targetType);
        }

        public override string Description
        {
            get
            {
                // var targetVar = GetTargetVariable();
                // if (targetVar == null)
                //     return "SetVarAction: No Target Variable";
                if (_sourceValueProvider == null)
                    return "SetVarAction: No Source Value Provider";

                // var targetDescription = _localVar != null ? _localVar.name : _targetVarProvider?.Description;
                return $"Set {_targetVarProvider?.Description} = {_sourceValueProvider.Description}";
            }
        }
    }
}

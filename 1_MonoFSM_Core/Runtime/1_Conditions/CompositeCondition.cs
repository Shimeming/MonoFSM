using MonoFSM.Variable.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _1_MonoFSM_Core.Runtime._1_Conditions
{
    public class CompositeCondition : AbstractConditionBehaviour
    {
        [Tooltip("選擇條件組合的邏輯操作：AND (所有條件都必須滿足) 或 OR (至少一個條件滿足)")]
        [LabelText("操作類型")]
        public CompositeOperationType _operationType = CompositeOperationType.And;

        [AutoChildren(DepthOneOnly = true, _isSelfInclude = false)]
        [CompRef]
        [RequiredListLength(2, null)]
        private AbstractConditionBehaviour[] _conditions;

        public override string Description => ReformatedName;

        protected override bool IsValid
        {
            get
            {
                if (_conditions == null || _conditions.Length == 0)
                    return true;

                switch (_operationType)
                {
                    case CompositeOperationType.And:
                        return _conditions.IsAllValid(this);

                    case CompositeOperationType.Or:
                        return _conditions.IsAnyValid(this);

                    default:
                        return _conditions.IsAllValid(this);
                }
            }
        }
    }
}

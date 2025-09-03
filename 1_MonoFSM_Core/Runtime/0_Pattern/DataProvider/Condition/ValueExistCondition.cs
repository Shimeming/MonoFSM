using MonoFSM.Foundation;
using UnityEngine;

namespace MonoFSM.Core.DataProvider.Condition
{
    public class ValueExistCondition : AbstractConditionBehaviour
    {
        public override string Description => $"Value Exist: {_targetValueGetter?.Description}";

        [DropDownRef]
        [SerializeField]
        private AbstractGetter _targetValueGetter;
        protected override bool IsValid => _targetValueGetter.HasValue;
    }
}

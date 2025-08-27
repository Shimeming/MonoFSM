using MonoFSM.Core.DataProvider;
using MonoFSM.Runtime.Attributes;

namespace MonoFSM.Variable.Condition
{
    public class ValueProviderBoolCondition : AbstractConditionBehaviour
    {
        [ValueTypeValidate(typeof(bool))]
        public ValueProvider _valueProvider;
        protected override bool IsValid => _valueProvider.Get<bool>();
        public override string Description => $"{_valueProvider.name} == true";
    }
}

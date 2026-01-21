using MonoFSM.Foundation;
using MonoFSM.Variable.Attributes;
using Sirenix.OdinInspector;

namespace MonoFSM.ValueSource.ValueSource
{
    //FIXME: 不能讓condition就？
    public class ConditionBoolValueSource : AbstractValueSource<bool>
    {
        [Required]
        [CompRef]
        [Auto]
        AbstractConditionBehaviour _condition;
        public override bool Value => _condition?.FinalResult ?? false;
    }
}

using MonoFSM.Variable.Attributes;
using Sirenix.OdinInspector;

namespace _1_MonoFSM_Core.Runtime._1_Conditions
{
    //錯了，應該是Vote??
    public class CompositeCondition : AbstractConditionBehaviour
    {
        //FIXME: 會需要開OR嗎？
        [AutoChildren(DepthOneOnly = true)]
        [CompRef]
        [RequiredListLength(2, null)]
        private AbstractConditionBehaviour[] _conditions;

        protected override bool IsValid => _conditions.IsAllValid();
    }
}

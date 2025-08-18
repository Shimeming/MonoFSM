using MonoFSM.Animation;
using MonoFSM.Variable.Attributes;
using Sirenix.OdinInspector;

namespace MonoFSM.Core
{

    public class AnimationDoneCondition : AbstractConditionBehaviour
    {
        protected override bool IsValid => _action.IsDone;
        //沒有serialized, 所以editor check會誤判..
        [Required] [CompRef] [AutoParent] private AnimatorPlayAction _action; //不用選的喔
        //FIXME: autosibiling? 啊好像不行如果有兩個AnimatorPlayAction
    }
}

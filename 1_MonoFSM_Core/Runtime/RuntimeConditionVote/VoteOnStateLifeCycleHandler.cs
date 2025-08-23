using MonoFSM.Core;
using UnityEngine;

namespace MonoFSM.Runtime.Vote
{
    /// <summary>
    ///     當和State一起投票..用個condition voter更單純? 還是跟著State好了
    ///     ConditionVoter
    ///     ---Condition(is state = Dead)
    /// </summary>
    public class VoteOnStateLifeCycleHandler : AbstractStateLifeCycleHandler
    {
        [DropDownRef]
        [SerializeField]
        private VarVote _vote;

        [SerializeField]
        private bool _voteValue = true;

        protected override void OnStateEnter()
        {
            base.OnStateEnter();
            // _vote.Vote
            _vote.Vote.Vote(_bindingState, _voteValue);
        }

        protected override void OnStateExit()
        {
            base.OnStateExit();
            _vote.Vote.Revoke(_bindingState);
        }
    }
}

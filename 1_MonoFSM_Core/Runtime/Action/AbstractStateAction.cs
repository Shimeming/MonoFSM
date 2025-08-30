using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MonoFSM.Core.Attributes;
using MonoFSM.Foundation;
using MonoFSM.Runtime;
using MonoFSM.Runtime.Vote;
using MonoFSMCore.Runtime.LifeCycle;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSM.Core.Runtime.Action
{
    /// <summary>
    ///     Represents an abstract base class for defining actions that are executed within a state
    ///     in the finite state machine (FSM) framework. Inherit from this class to implement
    ///     custom state actions.
    /// </summary>
    [Searchable]
    public abstract class AbstractStateAction
        : AbstractDescriptionBehaviour,
            IVoteChild,
            IGuidEntity,
            IDefaultSerializable,
            IEventReceiver,
            IResetStateRestore
    // IArgEventReceiver<GeneralEffectHitData>
    {
        // public MonoEntity ParentEntity => GetComponentInParent<MonoEntity>();
        public MonoEntity ParentEntity => bindingState.ParentEntity;

        protected override bool HasError()
        {
            return GetComponentInParent<IActionParent>(true) == null;
        }

        protected override string DescriptionTag => "Action";

        //怎麼知道誰用Enter, 誰用Update
        public bool IsValid //AND
        {
            get
            {
                if (_delay)
                    return false;
                return isActiveAndEnabled && _conditions.IsAllValid();
            }
        }

        // [PreviewInInspector]
        //FIXME: 不一定會有bindingState? 還是乾脆拿logic的就好了？
        [AutoParent]
        protected GeneralState bindingState; // => this.GetComponentInParent<GeneralState>(true)// ;

        [Required]
        [PreviewInInspector]
        [AutoParent]
        protected IActionParent _actionParent;

        [HideInInlineEditors]
        // #if UNITY_EDITOR
        [PropertyOrder(1)]
        [TabGroup("Condition", false, 1)]
        [Component(AddComponentAt.Children, "[Condition]")]
        [PreviewInInspector]
        // #endif
        [AutoChildren(DepthOneOnly = true)]
        protected AbstractConditionBehaviour[] _conditions; //condition 成立，才能做事
#if UNITY_EDITOR
        [PreviewInInspector]
        private bool IsAllValid => _conditions.IsAllValid();
#endif

        protected virtual string renamePostfix => "";

        [AutoParent]
        private DelayActionModifier delayActionModifier;

        private bool _delay; //FIXME:

        //FIXME: 不會走這了？
        public async void OnActionExecute()
        {
            if (!isActiveAndEnabled)
                return;
            if (_delay)
                Debug.LogError("Delay 還沒結束又DELAY 死罪", this);

            // _delay = false;
            //TODO: conditions
            if (!IsValid)
                return; //not valid也要用字串？

            _delay = true;
            if (delayActionModifier != null)
                try
                {
                    //FIXME: 這個delay用unitask不好，時間軸和fsm錯開了
                    //有點像sequence? 如果另外包好像還行？
                    await UniTask.Delay(
                        TimeSpan.FromSeconds(delayActionModifier.delayTime),
                        DelayType.DeltaTime,
                        PlayerLoopTiming.Update,
                        cancellationTokenSource.Token
                    );
                }
                catch (OperationCanceledException)
                {
                    _delay = false;
                    // Debug.LogError("Delay Cancelled" + e, this);
                    return;
                }

            _delay = false;
            // this.AddTask(OnStateEnterImplement, delayActionModifier.delayTime);
            _lastEventReceivedTime = Time.time;
            OnActionExecuteImplement();
            Debug.Log($"Action Executed: {name} {renamePostfix} at {_lastEventReceivedTime}", this);
        }

        protected abstract void OnActionExecuteImplement();

        [Obsolete]
        protected virtual void OnSpriteUpdateImplement() { }

        // public async void OnActionExit()
        // {
        //     if (!IsValid) return;
        //     if (delayActionModifier != null) await UniTask.Delay(TimeSpan.FromSeconds(delayActionModifier.delayTime));
        //     OnStateExitImplement();
        // }
        //
        // protected virtual void OnStateExitImplement()
        // {
        // }

        public virtual MonoBehaviour VoteOwner => nearestBinder as MonoBehaviour;

        [AutoParent]
        private IBinder nearestBinder;

        protected CancellationTokenSource cancellationTokenSource =>
            bindingState.GetStateExitCancellationTokenSource();

        //FIXME: 不該全部都virtual
        // public virtual void ArgEventReceived(IEffectHitData arg)
        // {
        //     EventReceived(arg);
        // }
        //
        // public virtual void ArgEventReceived(GeneralEffectHitData arg)
        // {
        //     EventReceived(arg);
        // }

        // public virtual void EventReceived<T>(T arg)
        // {
        //     OnActionExecuteImplement();
        // }
#if UNITY_EDITOR
        [PreviewInInspector]
        protected float _lastEventReceivedTime = -1f;
#endif

        public void EventReceived()
        {
            _lastEventReceivedTime = Time.time;
            // if (!isActiveAndEnabled)
            // if (enabled == false)
            //     Debug.LogError("not enabled", this);
            // if (gameObject.activeInHierarchy == false)
            //     Debug.LogError("not activeInHierarchy", this);
            if (gameObject.activeInHierarchy)
                OnActionExecuteImplement();
            // else
            //     Debug.LogError("Not active self", this);
        }

        public virtual void SimulationUpdate(float passedDuration) { }

        public virtual void SetPlaybackTime(float time) { }

        public virtual void Pause() { }

        public virtual void Resume() { }

        public virtual void ResetStateRestore()
        {
            _lastEventReceivedTime = -1f;
            _delay = false;
        }
    }
}

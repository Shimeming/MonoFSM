using _1_MonoFSM_Core.Runtime.FSMCore.Core.StateBehaviour;

namespace MonoFSM.Core
{
    // [Obsolete("Obsolete")]
    public class AnyState : MonoStateBehaviour, IState<GeneralState>, IDefaultSerializable
    {
        // [AutoParent] private GeneralFSMContext context;

        // public GeneralFSMContext Context => context;

        public bool TransitionCheck(
            GeneralState toState,
            float timeOffset = 0,
            StateTransition fromTransition = null
        )
        {
            // context.SetLastTransition(fromTransition);
            return TransitionCheck(toState);
        }

        public bool TransitionCheck(GeneralState toState)
        {
            // var fsm = context.fsm;
            // fsm.ChangeState(toState);

            return toState.ForceGoToState();
        }
        // public bool ForceTransition(GeneralState toState)
        // {
        //     var fsm = context.fsm;
        //     fsm.ChangeState(toState);
        //     return true;
        //     // return false;
        // }


        // [AutoChildren] StateTransition[] transitions;

#if UNITY_EDITOR
        // [Button("Add Event Transition")]
        // public void AddEventTransitionEditor()
        // {
        //     AddEventTransition();
        // }
        //
        // public RCGEventReceiveTransition AddEventTransition()
        // {
        //     Undo.RecordObject(this, "Add To Transition List");
        //     var t = gameObject.AddChildrenComponent<RCGEventReceiveTransition>("[Transition] NewTransition");
        //     // Undo.RegisterCompleteObjectUndo()
        //     // Undo.IncrementCurrentGroup();
        //     transitions.Add(t);
        //     // EditorUtility.SetDirty(this);
        //     return t;
        // }
#endif
    }
}

using MonoFSM.Core.DataProvider;
using MonoFSM.Core.Runtime.Action;
using MonoFSM.Core.Simulate;
using MonoFSM.Variable;
using UnityEngine;

namespace MonoFSM.Core
{
    public class ResetTimerAction : AbstractStateAction
    {
        [DropDownRef] public VarFloatCountDownTimer timer;

        protected override void OnActionExecuteImplement()
        {
            timer.ResetTimer();
        }
    }
}

using MonoFSM.Core.Runtime.Action;
using UnityEngine;

namespace RCGMakerFSMCore.Runtime.Action.DebugAction
{
    public class DebugBreakAction:AbstractStateAction
    {
        public bool _isForceBreak = true;
        protected override void OnActionExecuteImplement()
        {
            if (_isForceBreak)
            {
                Debug.LogError("Debug Break Action executed, breaking the game.", this);
                Debug.Break();
            }
            else
            {
                this.Break();
            }
        }
    }
}

using MonoFSM.Core.Runtime.Action;
using UnityEngine;
namespace RCGMakerFSMCore.Runtime.Action.DebugAction
{
    public class LogAction : AbstractStateAction, IArgEventReceiver<float>,
        IArgEventReceiver<string>
    {
        public string _logMessage = "LogAction";
        public bool _isLogInProvider = false;

        protected override void OnActionExecuteImplement()
        {
            if (_isLogInProvider)
                this.Log(_logMessage);
            else
                Debug.Log(_logMessage, this);
        }


        //這個會對嗎？
        public void ArgEventReceived(string arg)
        {
            Debug.Log($"LogAction ArgEventReceived: {arg}", this);
        }

        public void ArgEventReceived(float arg)
        {
            Debug.Log($"LogAction ArgEventReceived: {arg}", this);
        }
    }
}

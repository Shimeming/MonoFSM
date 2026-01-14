using MonoFSM.EditorExtension;
using UnityEngine;

namespace MonoFSM.Core.Runtime.Action
{
    //IEventInvoker?
    public interface IActionParent //給GameObject結構Validate用的
    { }

    //FIXME: 這個其實沒有想要implement actionExecute? 要把Implement override蓋掉？
    public abstract class AbstractArgEventHandler<T>
        : AbstractStateAction,
            IArgEventReceiver<T>,
            IHierarchyValueInfo
    // where T : IEffectHitData
    {
        void IArgEventReceiver<T>.ArgEventReceived(T arg)
        {
            //FIXME: 要做delay嗎？
            AddEventTime(Time.time);
            OnArgEventReceived(arg);
        }

        protected abstract void OnArgEventReceived(T arg);
#if UNITY_EDITOR
        public string ValueInfo => "evt:" + lastEventReceivedTime.ToString("F2");
        public bool IsDrawingValueInfo => lastEventReceivedTime != -1f;
#else
        public string ValueInfo => "";
        public bool IsDrawingValueInfo => false;
#endif
    }
}

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MonoFSM.Core.Attributes;
using MonoFSM.EditorExtension;
using MonoFSM.Foundation;
using MonoFSM.Runtime;
using MonoFSM.Runtime.Vote;
using MonoFSMCore.Runtime.LifeCycle;
using Sirenix.OdinInspector;
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
            _lastEventReceivedTime = Time.time;
            OnArgEventReceived(arg);
        }

        protected abstract void OnArgEventReceived(T arg);
        public string ValueInfo => _lastEventReceivedTime.ToString("F2");
        public bool IsDrawingValueInfo => _lastEventReceivedTime != -1f;
    }
}

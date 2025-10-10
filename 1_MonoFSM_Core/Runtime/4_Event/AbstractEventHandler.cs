using System.Threading;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.Runtime;
using MonoFSM.Core.Runtime.Action;
using MonoFSM.Foundation;
using MonoFSM.Variable.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSM.Core
{
    //各種事件的進入節點
    //ex: OnStateEnter, OnStateUpdate, OnStateExit
    //ex: OnEffectEnter, OnEffectExit
    //ex: OnPointerClick

    /// <summary>
    /// An abstract class that handles events and distributes them to registered event receivers.
    /// </summary>
    /// <remarks>
    /// This class is responsible for managing a collection of <see cref="IEventReceiver"/> components
    /// and triggering their event handling methods when an event occurs. It automatically finds
    /// and registers child event receivers through the <see cref="CompRef"/> and <see cref="AutoChildren"/>
    /// attributes.
    /// </remarks>
    /// <seealso cref="IEventReceiver"/>
    /// <seealso cref="IEventReceiver{T}"/>
    /// <seealso cref="IActionParent"/>
    public abstract class AbstractEventHandler : AbstractDescriptionBehaviour, IActionParent
    {
        protected override string DescriptionTag => "Event";

        public override string Description => GetType().Name.Replace("Handler", "");

        // GetType().Name.Replace("Handler", ""); //要都叫做 OnXXX ?

        [CompRef]
        [AutoChildren(DepthOneOnly = true)]
        protected IEventReceiver[] _eventReceivers; //IActions

        [InfoBox("目前不是所有EntityProvider都是合法的喔")]
        [CompRef]
        [AutoChildren(DepthOneOnly = true)]
        //FIXME: 要有篩選機制？靠Drawer去找囉？
        private AbstractEntityProvider[] _entityProviders;

        /// <summary>
        /// Call all event receivers' <see cref="IEventReceiver.EventReceived"/> method.
        /// </summary>
        public virtual void EventHandle()
        {
            // if (!isActiveAndEnabled) //FIXME: 打開的瞬間，我還沒打開？
            //     return;
            _lastEventHandledTime = Time.time;
            foreach (var eventReceiver in _eventReceivers)
            {
                if (eventReceiver.IsValid)
                    eventReceiver.EventReceived();
            }
        }



        [PreviewInInspector]
        private float _lastEventHandledTime = -1f;

        protected virtual void EventHandleImplement<T>(T arg)
        {
            _lastEventHandledTime = Time.time;
            foreach (var eventReceiver in _eventReceivers)
                //有參數的介面時
                if (eventReceiver is IArgEventReceiver<T> argEventReceiver)
                {
                    if (argEventReceiver.IsValid)
                        argEventReceiver.ArgEventReceived(arg); //在這裡delay?
                }
                else
                {
                    eventReceiver.EventReceived(); //在這裡delay?
                }
        }

        /// <summary>
        /// Call all event receivers' <see cref="IEventReceiver{T}.EventReceived"/> method with the given argument.
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="arg">The argument to pass to the event receivers.</param>
        public void EventHandle<T>(T arg)
        {
            //FIXME:會需要condition嗎?
            if (!isActiveAndEnabled)
                return;

            EventHandleImplement(arg);
        }
    }
}

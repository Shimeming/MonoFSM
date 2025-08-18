using System;
using MonoFSM.Core.Attributes;
using MonoFSM.Runtime.Interact.EffectHit;
using UnityEngine;

namespace MonoFSM.Runtime.Interact.SpatialDetection
{
    /// <summary>
    ///     重新整
    /// </summary>
    public class MouseOverDetectable : EffectDetectable
    {
        [Component]
        [AutoChildren(DepthOneOnly = true)]
        [PreviewInInspector]
        private AbstractConditionBehaviour[] _conditions =
            Array.Empty<AbstractConditionBehaviour>();

        public void OnMouseEnter()
        {
            // Debug.Log("OnMouseEnter", this);
            //可以顯示UI那類的
            if (!_conditions.IsAllValid())
            {
                Debug.Log("MouseDownDetectable Conditions not met", this);
                return;
            }

            //current mouse effectDealer?
            var detector = MouseDetector.Instance;
            // detector.QueueEnterEvent(gameObject);
            throw new NotImplementedException(
                "MouseOverDetectable OnMouseEnter not implemented yet.");
        }

        public void OnMouseExit()
        {
            var detector = MouseDetector.Instance;
            throw new NotImplementedException(
                "MouseOverDetectable OnMouseExit not implemented yet.");
            // detector.QueueExitEvent(gameObject);
        }
    }
}

using MonoFSM.Core.Runtime.Action;
using MonoFSM.Runtime.Interact.EffectHit;
using UnityEngine;

namespace MonoFSM_Physics.Runtime
{
    //情境出發，這樣就大家各自寫囉，可以有甚麼程度的模組化/覆用？
    public class ArrowHitAction : AbstractStateAction, IArgEventReceiver<GeneralEffectHitData>
    {
        public Rigidbody _rigidbody;

        protected override void OnActionExecuteImplement() { }

        public void ArgEventReceived(GeneralEffectHitData arg)
        {
            // Debug.Break();
            Debug.Log($"Arrow hit action received data: {arg}");
            if (arg.hitPoint == null)
                Debug.LogError("Hit point is null, cannot set rigidbody position.", this);
            else
                _rigidbody.position = arg.hitPoint.Value;
            // Debug.Break();
            //這樣共用實作？情境composite? 超廢
            // SetVelocityAction.SetVelocityToRigidbody(_rigidbody, Vector3.zero);
            _rigidbody.isKinematic = true;
            // _rigidbody.linearVelocity = Vector3.zero;
            // _rigidbody.angularVelocity = Vector3.zero;
            // _rigidbody.freezeRotation = true;
        }
    }
}

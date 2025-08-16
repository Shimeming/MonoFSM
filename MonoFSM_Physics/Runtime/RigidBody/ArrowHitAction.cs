using MonoFSM.Core.Runtime.Action;
using UnityEngine;

namespace MonoFSM_Physics.Runtime
{
    //情境出發，這樣就大家各自寫囉，可以有甚麼程度的模組化/覆用？
    public class ArrowHitAction : AbstractStateAction
    {
        public Rigidbody _rigidbody;

        protected override void OnActionExecuteImplement()
        {
            //這樣共用實作？情境composite? 超廢
            // SetVelocityAction.SetVelocityToRigidbody(_rigidbody, Vector3.zero);
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.freezeRotation = true;
            _rigidbody.isKinematic = true;
        }
    }
}

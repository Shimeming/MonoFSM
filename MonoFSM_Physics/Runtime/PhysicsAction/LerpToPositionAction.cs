using MonoFSM.Core.DataProvider;
using MonoFSM.Core.Runtime.Action;
using UnityEngine;

namespace MonoFSM_Physics.Runtime.PhysicsAction
{
    public class LerpToPositionAction : AbstractStateAction
    {
        // [Required] [CompRef] [AutoChildren] private ICompProvider<Rigidbody> _rigidbodyProvider;
        [DropDownRef] public ValueProvider _rigidbodyValueProvider;
        public Vector3 _offsetPosition = Vector3.zero;

        protected override void OnActionExecuteImplement()
        {
            var rb = _rigidbodyValueProvider.Get<Rigidbody>();
            // var rb = _rigidbodyProvider.Get();
            if (rb == null)
            {
                Debug.LogError("Rigidbody is null. Cannot perform LerpToPositionAction.", this);
                return;
            }

            Debug.Log($"LerpToPositionAction: {rb.name} to position {_offsetPosition}",
                rb.gameObject);
            rb.isKinematic = true;
            var targetPosition = rb.transform.position + _offsetPosition;
            //FIXME: network卡住？
            // rb.AddForce((targetPosition - rb.position) * 10, ForceMode.VelocityChange);
            rb.MovePosition(Vector3.Lerp(rb.position, targetPosition, bindingState.DeltaTime));
        }
    }
}

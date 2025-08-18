using MonoFSM.Core;
using MonoFSM.Core.DataProvider;
using UnityEngine;

namespace MonoFSM_Physics.Runtime.PhysicsAction
{
    public interface ICustomRigidbody
    {
        public void AddForce(Vector3 force, ForceMode mode);
        public Vector3 position { get; set; }
        public bool isPaused { set; get; }
    }

    public class LerpToPositionAction : AbstractStateLifeCycleHandler
    {
        // [Required] [CompRef] [AutoChildren] private ICompProvider<Rigidbody> _rigidbodyProvider;
        [DropDownRef] public ValueProvider _rigidbodyValueProvider;
        public Vector3 _offsetPosition = Vector3.zero;
        private Rigidbody _rb;

        protected override void OnStateEnter()
        {
            base.OnStateEnter();
            _rb = _rigidbodyValueProvider.Get<Rigidbody>();
            var rb = _rb;
            rb.isKinematic = true;
            _targetPosition = rb.transform.position + _offsetPosition;
            if (rb.TryGetComponent<ICustomRigidbody>(out var customRigidbody))
                customRigidbody.isPaused = true;
        }

        private Vector3 _targetPosition;

        protected override void OnStateUpdate()
        {
            base.OnStateUpdate();
            var rb = _rigidbodyValueProvider.Get<Rigidbody>();

            //character要另外處理...
            if (rb.TryGetComponent<ICustomRigidbody>(out var customRigidbody))
            {
                // customRigidbody.AddForce((_offsetPosition - rb.position) * 100,
                //     ForceMode.VelocityChange);
                customRigidbody.position = Vector3.Lerp(rb.position, _targetPosition,
                    DeltaTime);
                return;
            }

            // var rb = _rigidbodyProvider.Get();
            if (rb == null)
            {
                Debug.LogError("Rigidbody is null. Cannot perform LerpToPositionAction.", this);
                return;
            }



            //FIXME: network卡住？
            // var forceDirection = (targetPosition - rb.position).normalized;
            // Debug.Log($"Force direction: {forceDirection}", this);
            // rb.AddForce(forceDirection * 1, ForceMode.VelocityChange);
            rb.MovePosition(Vector3.Lerp(rb.position, _targetPosition, DeltaTime));
        }

        protected override void OnStateExit()
        {
            base.OnStateExit();
            _rb.isKinematic = false;
            if (_rb.TryGetComponent<ICustomRigidbody>(out var customRigidbody))
                customRigidbody.isPaused = false;
        }
    }
}

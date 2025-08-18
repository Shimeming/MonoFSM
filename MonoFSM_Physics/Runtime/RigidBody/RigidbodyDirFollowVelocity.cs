using MonoFSM.Core.Simulate;
using UnityEngine;

namespace MonoFSM_Physics.Runtime
{
    public class RigidbodyDirFollowVelocity : MonoBehaviour, IUpdateSimulate
    {
        [Auto] [SerializeField] private Rigidbody _rb;

        public void Simulate(float deltaTime)
        {
            if (_rb == null || _rb.isKinematic)
                return;
            var v = _rb.linearVelocity;
            if (v.sqrMagnitude > 0.01f)
            {
                var targetRot = Quaternion.LookRotation(v, Vector3.up);
                _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRot, 0.2f)); // 0.2f 可調
            }
        }

        public void AfterUpdate()
        {
            // throw new NotImplementedException();
        }
    }
}

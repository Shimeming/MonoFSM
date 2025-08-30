using UnityEngine;

namespace MonoFSM_Physics.Runtime.Interact.SpatialDetection
{
    public class DistanceProviderFromSpeed : MonoBehaviour
    {
        public Rigidbody _rigidbody;

        public float _minDis = 0.5f;

        //FIXME: init speed?
        //要用上個frame的速度嗎? 最好是把它記下來？
        public float Distance => _rigidbody.linearVelocity.magnitude + _minDis;
    }
}

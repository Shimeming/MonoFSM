using System;
using MonoFSM.Core.Runtime.Interact.SpatialDetection;
using UnityEngine;

namespace MonoFSM.PhysicsWrapper
{
    [Serializable]
    public class TransformForwardRayProvider : AbstractRayProvider
    {
        [SerializeField]
        private Transform _transform;
        private Rigidbody _rb;

        public override Ray GetRay()
        {
            // Create ray from camera through screen center
            if (_transform == null)
                _transform = transform; // Use the current GameObject's transform if not set
            var ray = new Ray(_transform.position, _transform.forward);
            // if (Application.isPlaying)
            //     Debug.Log($"Ray Origin: {ray.origin}, Direction: {ray.direction}", this);
            return ray;
        }
    }
}

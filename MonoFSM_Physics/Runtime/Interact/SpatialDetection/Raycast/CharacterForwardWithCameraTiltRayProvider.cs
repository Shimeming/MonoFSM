using MonoFSM.Core.Runtime.Interact.SpatialDetection;
using UnityEngine;

namespace MonoFSM.PhysicsWrapper
{
    //FIXME: 用photon projectile?
    public class CharacterForwardWithCameraTiltRayProvider : AbstractRayProvider
    {
        private Transform _characterTransform;

        [SerializeField]
        private float _minVerticalAngle = -45f; // Minimum vertical angle limit

        [SerializeField]
        private float _maxVerticalAngle = 45f;

        public override Ray GetRay()
        {
            _characterTransform = transform;
            var ray = new Ray(_characterTransform.position, _characterTransform.forward);
            //FIXME: DI camera?
            var mainCamera = Camera.main;
            // Get camera's pitch (vertical rotation)
            var cameraPitch = mainCamera.transform.eulerAngles.x;
            // Normalize angle to -180 to 180 range
            if (cameraPitch > 180f)
                cameraPitch -= 360f;

            // Clamp the pitch within our limits
            var clampedPitch = Mathf.Clamp(cameraPitch, _minVerticalAngle, _maxVerticalAngle);

            // Use the character's forward direction as the base
            var characterForward = _characterTransform.forward;
            var horizontalForward = new Vector3(
                characterForward.x,
                0,
                characterForward.z
            ).normalized;

            // Create rotation from the character's Y rotation (yaw)
            var characterYawRotation = Quaternion.Euler(0, _characterTransform.eulerAngles.y, 0);

            // Apply pitch rotation around the local X axis
            var pitchRotation = Quaternion.Euler(clampedPitch, 0, 0);

            // First apply character's yaw, then apply the camera pitch
            var newDirection = characterYawRotation * (pitchRotation * Vector3.forward);

            // Create a new ray with the adjusted direction
            ray = new Ray(ray.origin, newDirection);
            transform.forward = newDirection;

            return ray;
        }
        //FIXME:
    }
}

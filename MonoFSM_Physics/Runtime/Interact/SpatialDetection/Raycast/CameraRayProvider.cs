using MonoFSM.Core.Runtime.Interact.SpatialDetection;
using UnityEngine;

namespace MonoFSM.PhysicsWrapper
{
    public class CameraRayProvider : IRayProvider
    {
        // [SerializeField] private Camera _mainCamera;
        //FIXME: 從provider拿？
        private Transform _characterTransform;
        [SerializeField] private float _minVerticalAngle = -45f; // Minimum vertical angle limit

        [SerializeField] private float _maxVerticalAngle = 45f;

        public override Ray GetRay()
        {
            // var screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
            // if (_mainCamera == null) _mainCamera = Camera.main;
            // // Create ray from camera through screen center
            _characterTransform = transform;
            var ray = new Ray(_characterTransform.position, _characterTransform.forward);

            // var ray = _mainCamera.ScreenPointToRay(screenCenter);
            // return ray;
            // if (_characterTransform != null)
            // {
            //FIXME:
            // _mainCamera ??= Camera.main;
            var mainCamera = Camera.main;
            // Get camera's pitch (vertical rotation)
            var cameraPitch = mainCamera.transform.eulerAngles.x;
            // Normalize angle to -180 to 180 range
            if (cameraPitch > 180f)
                cameraPitch -= 360f;

            // Clamp the pitch within our limits
            var clampedPitch = Mathf.Clamp(
                cameraPitch,
                _minVerticalAngle,
                _maxVerticalAngle
            );

            // Use the character's forward direction as the base
            var characterForward = _characterTransform.forward;
            var horizontalForward = new Vector3(
                characterForward.x,
                0,
                characterForward.z
            ).normalized;

            // Create rotation from the character's Y rotation (yaw)
            var characterYawRotation = Quaternion.Euler(
                0,
                _characterTransform.eulerAngles.y,
                0
            );

            // Apply pitch rotation around the local X axis
            var pitchRotation = Quaternion.Euler(clampedPitch, 0, 0);

            // First apply character's yaw, then apply the camera pitch
            var newDirection = characterYawRotation * (pitchRotation * Vector3.forward);

            // Create a new ray with the adjusted direction
            ray = new Ray(ray.origin, newDirection);
            transform.forward = newDirection;
            // }

            // else
            // {
            //     var camera = Camera.main;
            //     if (camera != null)
            //     {
            //         // Get camera's pitch (vertical rotation)
            //         var cameraPitch = camera.transform.eulerAngles.x;
            //         // Normalize angle to -180 to 180 range
            //         if (cameraPitch > 180f)
            //             cameraPitch -= 360f;
            //
            //         // Clamp the pitch within our limits
            //         var clampedPitch = Mathf.Clamp(
            //             cameraPitch,
            //             _minVerticalAngle,
            //             _maxVerticalAngle
            //         );
            //
            //         // Default implementation when character transform is not set
            //         // Create a new direction that preserves horizontal direction but applies vertical angle
            //         var horizontalDir = new Vector3(
            //             camera.transform.forward.x,
            //             0,
            //             camera.transform.forward.z
            //         ).normalized;
            //
            //         // Apply pitch rotation to the horizontal direction
            //         var pitchRotation = Quaternion.Euler(clampedPitch, 0, 0);
            //         var newDirection = pitchRotation * Vector3.forward;
            //
            //         // Create a new ray with the adjusted direction
            //         ray = new Ray(ray.origin, newDirection);
            //     }
            // }
            // Debug.Log("camera ray" + ray.direction, this);
            return ray;
        }
        //FIXME:

    }
}

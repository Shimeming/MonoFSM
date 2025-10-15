using MonoFSM.Core.Runtime.Interact.SpatialDetection;
using UnityEngine;

namespace MonoFSM.PhysicsWrapper
{
    public class CameraDirectionRayProvider : AbstractRayProvider
    {
        public override Ray GetRay()
        {
            //會打到自己？
            var mainCamera = Camera.main;
            return new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        }
    }
}

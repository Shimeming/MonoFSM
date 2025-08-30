using MonoFSM.Core.Runtime.Interact.SpatialDetection;
using MonoFSM.Foundation;
using UnityEngine;

namespace MonoFSM.PhysicsWrapper
{
    public class Vec3FromRayDirProvider : AbstractValueProvider<Vector3>
    {
        [DropDownRef]
        [SerializeField]
        private RaycastCache _rayCache;
        public override string Description => "Vec3 dir:" + _rayCache?.name;
        public override Vector3 Value => _rayCache.CachedRay.direction;
    }
}

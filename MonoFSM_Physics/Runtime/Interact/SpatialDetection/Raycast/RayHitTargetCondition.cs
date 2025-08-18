using MonoFSM.Core.Runtime.Interact.SpatialDetection;
using UnityEngine.Serialization;

namespace MonoFSM.PhysicsWrapper
{
    public class RayHitTargetCondition : AbstractConditionBehaviour
    {
        [FormerlySerializedAs("_detector")] [DropDownRef]
        public RaycastCache _cache;

        //沒有更新？
        protected override bool IsValid => _cache.CachedHit.collider != null;
    }
}

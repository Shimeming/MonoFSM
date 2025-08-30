using UnityEngine;
using UnityEngine.Internal;

namespace MonoFSM.PhysicsWrapper
{
    public interface IAllInOneRaycastProcessor
        : IRaycastProcessor,
            ISphereCastProcessor,
            ICapsuleRaycastProcessor,
            IBoxCastProcessor { }

    public interface IRaycastProcessor
    {
        bool Raycast(Vector3 origin, Vector3 direction, float maxDistance);
        bool Raycast(Vector3 origin, Vector3 direction, float maxDistance, int layerMask);
        bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance);
        bool Raycast(
            Vector3 origin,
            Vector3 direction,
            out RaycastHit hitInfo,
            float maxDistance,
            int layerMask
        );

        bool Raycast(
            Vector3 origin,
            Vector3 direction,
            out RaycastHit hitInfo,
            float maxDistance,
            int layerMask,
            QueryTriggerInteraction queryTriggerInteraction
        );

        public int RaycastNonAlloc(
            Vector3 origin,
            Vector3 direction,
            RaycastHit[] hitInfos,
            float maxDistance,
            int layerMask,
            QueryTriggerInteraction queryTriggerInteraction
        );

        // RaycastHit[] RaycastAll(Vector3 origin, Vector3 direction, float maxDistance,
        //     int layerMask, QueryTriggerInteraction queryTriggerInteraction);
        //
        // int RaycastNonAlloc(Vector3 origin, Vector3 direction, RaycastHit[] results,
        //     float maxDistance, int layerMask,
        //     QueryTriggerInteraction queryTriggerInteraction);
    }

    public interface ISphereCastProcessor
    {
        int SphereOverlap(
            Vector3 origin,
            float radius,
            Collider[] results,
            [DefaultValue("DefaultRaycastLayers")] int layerMask,
            [DefaultValue("QueryTriggerInteraction.UseGlobal")]
                QueryTriggerInteraction queryTriggerInteraction
        );
        bool SphereCast(
            Vector3 origin,
            float radius,
            Vector3 direction,
            out RaycastHit hitInfo,
            [DefaultValue("Mathf.Infinity")] float maxDistance,
            [DefaultValue("DefaultRaycastLayers")] int layerMask,
            [DefaultValue("QueryTriggerInteraction.UseGlobal")]
                QueryTriggerInteraction queryTriggerInteraction
        );
        public int SphereCastNonAlloc(
            Vector3 origin,
            float radius,
            Vector3 direction,
            RaycastHit[] results,
            [DefaultValue("Mathf.Infinity")] float maxDistance,
            [DefaultValue("DefaultRaycastLayers")] int layerMask,
            [DefaultValue("QueryTriggerInteraction.UseGlobal")]
                QueryTriggerInteraction queryTriggerInteraction
        );
        // bool SphereCast(Vector3 origin, Vector3 direction, float radius, float maxDistance);
        // bool SphereCast(Vector3 origin, Vector3 direction, float radius, float maxDistance, int layerMask);
        //
        // bool SphereCast(Vector3 origin, Vector3 direction, float radius, out RaycastHit hitInfo,
        //     float maxDistance);
        //
        // bool SphereCast(Vector3 origin, Vector3 direction, float radius, out RaycastHit hitInfo,
        //     float maxDistance, int layerMask);
        //
        // bool SphereCast(Vector3 origin, Vector3 direction, float radius, out RaycastHit hitInfo,
        //     float maxDistance,
        //     int layerMask, QueryTriggerInteraction queryTriggerInteraction);
    }
}

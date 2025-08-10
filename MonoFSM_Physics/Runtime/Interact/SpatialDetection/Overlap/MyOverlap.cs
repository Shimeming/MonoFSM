using UnityEngine;
using UnityEngine.Internal;

namespace MonoFSM.PhysicsWrapper
{
    
    public interface ICapsuleOverlapProcessor
    {
        public int OverlapCapsuleNonAlloc(
            Vector3 point1,
            Vector3 point2,
            float radius,
            Collider[] results,
            [DefaultValue("DefaultRaycastLayers")] int layerMask,
            [DefaultValue("QueryTriggerInteraction.UseGlobal")]
            QueryTriggerInteraction queryTriggerInteraction  = QueryTriggerInteraction.UseGlobal);
    }


    public class MyOverlap : MonoBehaviour
    {
        [Auto] private IOverlapProcessor _overlapProcessor;

        /// <summary>
        /// Find all colliders touching or inside the capsule.
        /// </summary>
        /// <param name="point1">The center of the sphere at the start of the capsule.</param>
        /// <param name="point2">The center of the sphere at the end of the capsule.</param>
        /// <param name="radius">The radius of the spheres and capsule.</param>
        /// <param name="results">The buffer to store the results in.</param>
        /// <param name="layerMask">A layer mask that is used to selectively ignore colliders when casting a capsule.</param>
        /// <param name="queryTriggerInteraction">Specifies whether this query should hit Triggers.</param>
        /// <returns>The number of colliders stored in the results buffer.</returns>
        public int OverlapCapsuleNonAlloc(Vector3 point1, Vector3 point2, float radius, Collider[] results, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            if (_overlapProcessor != null) 
                return _overlapProcessor.OverlapCapsuleNonAlloc(point1, point2, radius, results, layerMask, queryTriggerInteraction);

            // Fallback to Physics.OverlapCapsuleNonAlloc if no processor is available
            
            return Physics.OverlapCapsuleNonAlloc(point1, point2, radius, results, layerMask, queryTriggerInteraction);
        }
        

        /// <summary>
        /// Find all colliders touching or inside the sphere.
        /// </summary>
        /// <param name="position">Center of the sphere.</param>
        /// <param name="radius">Radius of the sphere.</param>
        /// <param name="results">The buffer to store the results in.</param>
        /// <param name="layerMask">A layer mask that is used to selectively ignore colliders when casting a sphere.</param>
        /// <param name="queryTriggerInteraction">Specifies whether this query should hit Triggers.</param>
        /// <returns>The number of colliders stored in the results buffer.</returns>
        public int OverlapSphereNonAlloc(Vector3 position, float radius, Collider[] results, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            if (_overlapProcessor != null) 
                return _overlapProcessor.OverlapSphereNonAlloc(position, radius, results, layerMask, queryTriggerInteraction);

            // Fallback to Physics.OverlapSphereNonAlloc if no processor is available
            return Physics.OverlapSphereNonAlloc(position, radius, results, layerMask, queryTriggerInteraction);
        }

        /// <summary>
        /// Find all colliders touching or inside the box.
        /// </summary>
        /// <param name="center">Center of the box.</param>
        /// <param name="halfExtents">Half the size of the box in each dimension.</param>
        /// <param name="results">The buffer to store the results in.</param>
        /// <param name="orientation">Rotation of the box.</param>
        /// <param name="layerMask">A layer mask that is used to selectively ignore colliders when casting a box.</param>
        /// <param name="queryTriggerInteraction">Specifies whether this query should hit Triggers.</param>
        /// <returns>The number of colliders stored in the results buffer.</returns>
        public int OverlapBoxNonAlloc(Vector3 center, Vector3 halfExtents, Collider[] results, Quaternion orientation = default, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            if (_overlapProcessor != null) 
                return _overlapProcessor.OverlapBoxNonAlloc(center, halfExtents, results, orientation, layerMask, queryTriggerInteraction);

            // Fallback to Physics.OverlapBoxNonAlloc if no processor is available
            return Physics.OverlapBoxNonAlloc(center, halfExtents, results, orientation, layerMask, queryTriggerInteraction);
        }
        
    }
}
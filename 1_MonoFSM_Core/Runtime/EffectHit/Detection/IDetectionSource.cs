using System.Collections.Generic;
using UnityEngine;

namespace MonoFSM.Core.Detection
{
    public struct DetectionResult
    {
        public GameObject targetObject;
        public Vector3? hitPoint;
        public Vector3? hitNormal;
        public bool isValidHit;

        public DetectionResult(
            GameObject target,
            Vector3? hitPoint = null,
            Vector3? hitNormal = null
        )
        {
            this.targetObject = target;
            this.hitPoint = hitPoint;
            this.hitNormal = hitNormal;
            this.isValidHit = target != null;
        }

        public static DetectionResult Invalid => new DetectionResult { isValidHit = false };
    }

    public interface IDetectionSource //AbstractComponent
    {
        bool IsEnabled { get; }

        IEnumerable<DetectionResult> GetCurrentDetections();

        void UpdateDetection();
    }
}

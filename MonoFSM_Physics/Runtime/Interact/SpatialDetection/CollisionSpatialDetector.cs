using System;
using System.Collections.Generic;
using MonoFSM.Core.Runtime.Interact.SpatialDetection;
using MonoFSM.Variable.Attributes;
using UnityEngine;

namespace MonoFSM.Core.Detection
{
    public class CollisionSpatialDetector : BaseDetectProcessor
    {
        [CompRef]
        [AutoChildren(DepthOneOnly = true)]
        private CollisionEventNode _enterNode;

        private void OnCollisionEnter(Collision other)
        {
            _enterNode?.EventHandle(other);
            _detector.OnDetectEnterCheck(other.gameObject);
        }

        private void OnCollisionExit(Collision other)
        {
            _detector.OnDetectExitCheck(other.gameObject);
        }

        //FIXME:
        // protected override void OnDisableImplement()
        // {
        // }
        //
        // protected override void SetLayerOverride()
        // {
        //     throw new System.NotImplementedException();
        // }

        public override IEnumerable<DetectionResult> GetCurrentDetections()
        {
            throw new NotImplementedException();
        }

        public override void UpdateDetection()
        {
            throw new NotImplementedException();
        }
    }
}

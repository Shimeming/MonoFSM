using System;
using System.Collections.Generic;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.Runtime.Interact.SpatialDetection;
using MonoFSM.Variable.Attributes;
using UnityEngine;

namespace MonoFSM.Core.Detection
{
    public class TriggerSpatialDetector2D : BaseDetectProcessor, IDetectionSource
    {
        [CompRef]
        [Auto]
        Collider2D _collider;

        private void OnTriggerEnter2D(Collider2D other)
        {
            _detector.OnDetectEnterCheck(other.gameObject);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            _detector.OnDetectExitCheck(other.gameObject);
        }

        // protected override void OnDisableImplement()
        // {
        // }
        //
        // protected override void SetLayerOverride()
        // {
        //     _collider.includeLayers = HittingLayer;
        // }

        public override IEnumerable<DetectionResult> GetCurrentDetections()
        {
            throw new NotImplementedException();
        }

        //FIXME: 不一定需要？
        public override void UpdateDetection()
        {
            // throw new NotImplementedException();
        }
    }
}

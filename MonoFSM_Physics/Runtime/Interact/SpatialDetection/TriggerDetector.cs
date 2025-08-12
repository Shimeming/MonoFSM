using System.Collections.Generic;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.Runtime.Interact.SpatialDetection;
using MonoFSM.Variable.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSM.Core.Detection
{
    public class TriggerDetector : BaseDetectProcessor, IDetectionSource
    {
        [Required]
        [CompRef]
        [Auto]
        private Collider _collider;

        [ShowInInspector]
        [AutoParent]
        private Rigidbody _rigidbodyInParent;

        [ShowIf("@_rigidbodyInParent == null")]
        [CompRef]
        [Auto]
        Rigidbody _optionalRigidbody;

        private HashSet<GameObject> _triggeredObjects = new();
        public bool IsEnabled => enabled;

        private void OnTriggerEnter(Collider other)
        {
            this.Log("OnTriggerEnter", other);
            _triggeredObjects.Add(other.gameObject);
            QueueEnterEvent(other.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            this.Log("OnTriggerExit", other);
            _triggeredObjects.Remove(other.gameObject);
            QueueExitEvent(other.gameObject);
        }

        public override IEnumerable<DetectionResult> GetCurrentDetections()
        {
            foreach (var obj in _triggeredObjects)
            {
                if (obj != null)
                    yield return new DetectionResult(obj);
            }
        }

        public override void UpdateDetection()
        {
            _triggeredObjects.RemoveWhere(obj => obj == null); //這個會遇到嗎...?
            ProcessEnterExitEvents();
        }

        //FIXME: Gizmo?
    }
}

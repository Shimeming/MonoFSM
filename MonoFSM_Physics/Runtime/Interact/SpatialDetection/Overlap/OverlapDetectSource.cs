using System.Collections.Generic;
using MonoFSM.Core.Detection;
using MonoFSM.PhysicsWrapper;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace MonoFSM_Physics.Runtime.Interact.SpatialDetection
{
    public class OverlapDetectSource : AbstractDetectionSource
    {
        [FormerlySerializedAs("_myOverlap")]
        [DropDownRef]
        public MyOverlap _overlapProcessor;

        [Header("Overlap Settings")]
        public OverlapShape _overlapShape = OverlapShape.Sphere;

        [ShowIf("@_overlapShape == OverlapShape.Sphere")]
        public float _radius = 1f;

        [ShowIf("@_overlapShape == OverlapShape.Box")]
        public Vector3 _halfExtents = Vector3.one * 0.5f;

        [ShowIf("@_overlapShape == OverlapShape.Capsule")]
        public float _capsuleRadius = 0.5f;

        [ShowIf("@_overlapShape == OverlapShape.Capsule")]
        public float _capsuleHeight = 2f;

        public LayerMask _layerMask = -1;
        public QueryTriggerInteraction _queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;

        private readonly Collider[] _overlapResults = new Collider[64];

        public enum OverlapShape
        {
            Sphere,
            Box,
            Capsule,
        }

        public override IEnumerable<DetectionResult> GetCurrentDetections()
        {
            if (_overlapProcessor == null)
                yield break;

            var hitCount = PerformOverlap();

            for (var i = 0; i < hitCount; i++)
            {
                var col = _overlapResults[i];
                var targetObject = col.attachedRigidbody
                    ? col.attachedRigidbody.gameObject
                    : col.gameObject;

                // Overlap 沒有 hit point 和 normal，所以用 collider center
                var hitPoint = col.bounds.center;
                yield return new DetectionResult(targetObject, hitPoint);
            }
        }

        public override void UpdateDetection()
        {
            PhysicsUpdate();
        }

        private void PhysicsUpdate()
        {
            _thisFrameColliders.Clear();

            if (_overlapProcessor == null)
                return;

            var hitCount = PerformOverlap();

            // 收集這一frame的colliders
            for (var i = 0; i < hitCount; i++)
                _thisFrameColliders.Add(_overlapResults[i]);

            // 檢查進入事件：上個frame不在，這個frame在
            foreach (var col in _thisFrameColliders)
                if (!_lastFrameColliders.Contains(col))
                {
                    var targetObject = col.attachedRigidbody
                        ? col.attachedRigidbody.gameObject
                        : col.gameObject;

                    var hitPoint = col.bounds.center;
                    _detector.OnDetectEnterCheck(targetObject, hitPoint);
                }

            // 檢查離開事件：上個frame在，這個frame不在
            foreach (var col in _lastFrameColliders)
                if (!_thisFrameColliders.Contains(col))
                {
                    var rb = col.attachedRigidbody;
                    if (rb == null)
                    {
                        rb = col.GetComponentInParent<Rigidbody>(true);
                        if (rb == null)
                            continue; // 跳過這個 collider
                    }

                    _detector.OnDetectExitCheck(rb.gameObject);
                }

            // 更新 frame 記錄
            _lastFrameColliders.Clear();
            _lastFrameColliders.AddRange(_thisFrameColliders);
        }

        private int PerformOverlap()
        {
            var position = transform.position;

            return _overlapShape switch
            {
                OverlapShape.Sphere => _overlapProcessor.OverlapSphereNonAlloc(
                    position,
                    _radius,
                    _overlapResults,
                    _layerMask,
                    _queryTriggerInteraction
                ),

                OverlapShape.Box => _overlapProcessor.OverlapBoxNonAlloc(
                    position,
                    _halfExtents,
                    _overlapResults,
                    transform.rotation,
                    _layerMask,
                    _queryTriggerInteraction
                ),

                OverlapShape.Capsule => _overlapProcessor.OverlapCapsuleNonAlloc(
                    position + Vector3.up * (_capsuleHeight * 0.5f - _capsuleRadius),
                    position + Vector3.down * (_capsuleHeight * 0.5f - _capsuleRadius),
                    _capsuleRadius,
                    _overlapResults,
                    _layerMask,
                    _queryTriggerInteraction
                ),

                _ => 0,
            };
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            var position = transform.position;

            switch (_overlapShape)
            {
                case OverlapShape.Sphere:
                    Gizmos.DrawWireSphere(position, _radius);
                    break;

                case OverlapShape.Box:
                    Gizmos.matrix = Matrix4x4.TRS(position, transform.rotation, Vector3.one);
                    Gizmos.DrawWireCube(Vector3.zero, _halfExtents * 2);
                    Gizmos.matrix = Matrix4x4.identity;
                    break;

                case OverlapShape.Capsule:
                    // 簡化的膠囊繪製
                    var point1 = position + Vector3.up * (_capsuleHeight * 0.5f - _capsuleRadius);
                    var point2 = position + Vector3.down * (_capsuleHeight * 0.5f - _capsuleRadius);
                    Gizmos.DrawWireSphere(point1, _capsuleRadius);
                    Gizmos.DrawWireSphere(point2, _capsuleRadius);
                    break;
            }
        }
    }
}

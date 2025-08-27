using System.Collections.Generic;
using MonoFSM.Core.Detection;
using MonoFSM.Core.Runtime.Interact.SpatialDetection;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace MonoFSM_Physics.Runtime.Interact.SpatialDetection
{
    public class RaycastDetectSource : IDetectionSource
    {
        //FIXME: 應該用他的 layerMask
        [FormerlySerializedAs("_raycastDetector")]
        [DropDownRef]
        public RaycastCache _raycastCache;

        public override IEnumerable<DetectionResult> GetCurrentDetections()
        {
            var _cachedHits = _raycastCache.CachedHits;
            foreach (var hit in _cachedHits)
            {
                var targetObject = hit.rigidbody
                    ? hit.rigidbody.gameObject
                    : hit.collider.gameObject;
                yield return new DetectionResult(targetObject, hit.point, hit.normal);
            }
        }

        public override void UpdateDetection()
        {
            //FIXME:  應該也用queue的方式處理enter exit事件？
            //現在OnDetectEnterCheck比較係可以傳細節， Queued不行
            PhysicsUpdate();
        }

        //
        // private void OnDisable()
        // {
        //     //FIXME: 要讓 EffectDetector handle就好？
        //     //TODO: Exit check?
        //     _thisFrameColliders.Clear();
        //     _lastFrameColliders.Clear();
        // }

        private void PhysicsUpdate() //network?
        {
            _thisFrameColliders.Clear();
            //FIXME:  TryCast();
            //從hit拿collider
            var cachedHits = _raycastCache.CachedHits;
            foreach (var hit in cachedHits)
                _thisFrameColliders.Add(hit.collider);

            //上個frame不在，這個frame卻在
            foreach (var hit in cachedHits)
                if (!_lastFrameColliders.Contains(hit.collider))
                {
                    // QueueEnterEvent(hit.collider.gameObject);
                    Debug.Log("Raycast enter: Detector Hit" + hit.collider.gameObject, this);
                    Debug.Log("Raycast enter: hitPoint " + hit.collider, hit.collider);

                    //Note: Detectable必須在 rigidbody上面？
                    //FIXME: 都遇hit.collider就好？
                    // if (hit.rigidbody)
                    //     _detector.OnDetectEnterCheck(hit.rigidbody.gameObject, hit.point, hit.normal);
                    // else
                    var result = _detector.OnDetectEnterCheck(
                        hit.collider.gameObject,
                        hit.point,
                        hit.normal
                    );
                    Debug.Log("Detect:" + hit.collider + result, this);
                }

            foreach (var col in _lastFrameColliders)
                if (!_thisFrameColliders.Contains(col))
                {
                    //FIXME: 已經關掉的話...是不是悲劇了？ rigidbody拿不到？
                    var rb = col.attachedRigidbody;
                    if (rb == null)
                    {
                        // Debug.LogError(
                        //     "RaycastDetector: Collider has no attached Rigidbody, cannot call OnSpatialExit.", col);
                        rb = col.GetComponentInParent<Rigidbody>(true);
                        if (rb == null)
                            // Debug.LogError(
                            //     "RaycastDetector: Collider has no attached Rigidbody or parent Rigidbody, cannot call OnSpatialExit.",
                            //     col
                            // );
                            continue; //跳過這個 collider
                    }

                    _detector.OnDetectExitCheck(rb.gameObject); //gameObject錯了...哭
                }

            _lastFrameColliders.Clear();
            _lastFrameColliders.AddRange(_thisFrameColliders);
        }
    }
}

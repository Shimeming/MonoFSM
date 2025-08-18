using System;
using System.Collections.Generic;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.Simulate;
using MonoFSM.Foundation;
using MonoFSM.PhysicsWrapper;
using MonoFSM.Variable.Attributes;
using MonoFSMCore.Runtime.LifeCycle;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace MonoFSM.Core.Runtime.Interact.SpatialDetection
{
    // [DisallowMultipleComponent]

    /// <summary>
    ///     純做Raycast的偵測器，會在SimulateUpdate時進行射線檢測。
    /// </summary>
    [DefaultExecutionOrder(-1)] //要把RaycastDetectSource前面，有執行順序問題hmm
    public class RaycastCache : AbstractDescriptionBehaviour, IUpdateSimulate, IResetStateRestore
    {
        public enum RaycastMode
        {
            Single, //FIXME: 應該都要用all 然後再sort, 然後會需要filter掉一部分
            All //會需要all嗎？這樣對象要全部分開？
        }

        [SerializeField]
        private RaycastMode _raycastMode = RaycastMode.Single;
        public float _distance = 30;

        [FormerlySerializedAs("HittingLayer")]
        [CustomSerializable]
        // [ShowInInspector]
        // [Required]
        public LayerMask _hittingLayer;

        //FIXME: validate 不可以是nothing? 或是直接收斂掉？

        private RaycastHit[] _allocHits = new RaycastHit[10]; //FIXME: 這個大小要怎麼處理？會不會有問題？ 這個是用來儲存raycast的結果

        private Collider[] _allocColliders = new Collider[10]; //FIXME: 這個大小要怎麼處理？會不會有問題？ 這個是用來儲存raycast的結果

        //用spherecast還是raycast？ spherecast會有問題嗎？

        [PreviewInInspector]
        private Collider firstHitCollider => CachedHits.Count > 0 ? CachedHits[0].collider : null;

        [PreviewInInspector] public List<RaycastHit> CachedHits { get; } = new();

        public RaycastHit CachedHit => CachedHits.Count > 0 ? CachedHits[0] : default;
        public Ray CachedRay => _cachedRay;


        [Auto] [CompRef]
        private IRaycastProcessor _raycastProcessor;

        // [CompRef] //all in 1 就撞了？
        // [Auto]
        // private ISphereCastProcessor _sphereCastProcessor;
        // public float _sphereRadius = 0.5f; //FIXME: spherecast的半徑要怎麼處理？ 這個是用來儲存spherecast的結果
        private Ray _cachedRay;

#if UNITY_EDITOR
        [ShowInDebugMode] private readonly List<Collider> _debugHistoryObjs = new();
#endif




        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            // if (_sphereCastProcessor != null)
            // {
            //     Gizmos.DrawWireSphere(_cachedRay.origin, _sphereRadius);
            //     Gizmos.DrawRay(_cachedRay.origin, _cachedRay.direction * _distance);
            //     Gizmos.DrawWireSphere(
            //         _cachedRay.origin + _cachedRay.direction * _distance,
            //         _sphereRadius
            //     );
            // }
            // else

            //FIXME: 處理 editor mode的ray provider
            if (Application.isPlaying == false && _rayProvider != null)
                _cachedRay = _rayProvider.GetRay();

            //FIXME: 要選mode? sphere cast, ray cast...
            Gizmos.DrawRay(_cachedRay.origin, _cachedRay.direction * _distance);

            foreach (var hit in CachedHits)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(hit.point, 0.1f);
            }
            // if (_cacehdHit.collider != null)
            // {
            //     Gizmos.color = Color.green;
            //     Gizmos.DrawSphere(_cacehdHit.point, 0.1f);
            // }
        }

        // CameraRayProvider
        //FIXME:
        // public bool _isEffectByCameraRotation;

        [SerializeField]
        private float _minVerticalAngle = -45f; // Minimum vertical angle limit

        [SerializeField]
        private float _maxVerticalAngle = 45f; // Maximum vertical angle limit
        // private Transform _characterTransform; // Reference to the character's transform

        void TryCast()
        {
            //FIXME: 把ray 外包？
            var ray = _rayProvider.GetRay();
            // _characterTransform = transform;

            CachedHits.Clear();
            // _thisFrameColliders.Clear();

            _cachedRay = ray;
            if (_raycastMode == RaycastMode.Single)
            {
                // if (_sphereCastProcessor != null)
                // {
                //     if (
                //         _sphereCastProcessor.SphereCast(
                //             ray.origin,
                //             _sphereRadius,
                //             ray.direction,
                //             out var hitInfo,
                //             _distance,
                //             _hittingLayer,
                //             QueryTriggerInteraction.UseGlobal
                //         )
                //     )
                //     {
                //         if (hitInfo.distance == 0)
                //             Debug.LogError(
                //                 "RaycastDetector: SphereCast hit distance is 0, this may indicate an issue with the ray or sphere radius.",
                //                 this
                //             );
                //         _cachedHits.Add(hitInfo);
                //         _thisFrameColliders.Add(hitInfo.collider);
                //     }
                // }
                // else
                if (_raycastProcessor != null)
                {
                    if (
                        _raycastProcessor.Raycast(
                            ray.origin,
                            ray.direction,
                            out var hitInfo,
                            _distance,
                            _hittingLayer
                        )
                    )
                    {
                        //FIXME: 操作 list好嗎？
                        CachedHits.Add(hitInfo);
                        // _thisFrameColliders.Add(hitInfo.collider);
                        _debugHistoryObjs.Add(hitInfo.collider);
                        // Debug.Log("hit" + hit.collider.name, hit.collider);
                    }
                }
                else if (Physics.Raycast(ray, out var hit, _distance, _hittingLayer))
                {
                    CachedHits.Add(hit);
                    // _thisFrameColliders.Add(hit.collider);
                    _debugHistoryObjs.Add(hit.collider);
                    // Debug.Log("hit" + hit.collider.name, hit.collider);
                }
            }
            else
            {
                throw new ArgumentNullException("No Multiple Raycast");
            }
            // else
            // {
            //     var hits = Physics.RaycastAll(ray, _distance, HittingLayer);
            //     foreach (var h in hits)
            //     {
            //         _cachedHits.Add(h);
            //         _thisFrameColliders.Add(h.collider);
            //         Debug.Log("hit" + h.collider.name, h.collider);
            //     }
            // }
        }




        [Required]
        [Auto]
        [CompRef]
        private IRayProvider _rayProvider;

        //update?
        // public void Simulate(float deltaTime)
        // {
        //     PhysicsUpdate();
        // }

        public void Simulate(float deltaTime)
        {
            TryCast();
        }

        public void AfterUpdate() { }

        protected override string DescriptionTag => "Raycast";

        public void ResetStateRestore()
        {
            //把狀態清掉
            _cachedRay = default;
            CachedHits.Clear();
            _debugHistoryObjs.Clear();
        }
    }

    public abstract class IRayProvider : MonoBehaviour
    {
        public abstract Ray GetRay();
    }
}

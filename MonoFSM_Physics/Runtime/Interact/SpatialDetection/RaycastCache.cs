using System;
using System.Collections.Generic;
using MonoFSM_Physics.Runtime.Interact.SpatialDetection;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.Simulate;
using MonoFSM.EditorExtension;
using MonoFSM.Foundation;
using MonoFSM.PhysicsWrapper;
using MonoFSM.Variable.Attributes;
using MonoFSMCore.Runtime.LifeCycle;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace MonoFSM.Core.Runtime.Interact.SpatialDetection
{
    /// <summary>
    ///     純做Raycast的偵測器，會在SimulateUpdate時進行射線檢測。
    /// </summary>
    [DefaultExecutionOrder(-1)] //要把RaycastDetectSource前面，有執行順序問題hmm
    public class RaycastCache
        : AbstractDescriptionBehaviour,
            IBeforeSimulate,
            IUpdateSimulate,
            IResetStateRestore,
            IHierarchyValueInfo
    {
        [SerializeField]
        private Transform _cacheOrigin;

        [SerializeField]
        private Transform _cacheEndPoint;

        public enum RaycastMode
        {
            Single, //FIXME: 應該都要用all 然後再sort, 然後會需要filter掉一部分
            All //會需要all嗎？這樣對象要全部分開？
            ,
        }

        [SerializeField]
        private RaycastMode _raycastMode = RaycastMode.Single;

        [HideIf("@_distanceProvider != null")]
        public float _distance = 30; //要依照速度來決定distance...distance provider?

        [FormerlySerializedAs("_distanceProvider")]
        [CompRef]
        [Auto]
        [SerializeField]
        private DistanceSourceFromSpeed _distanceSource;

        [ShowInInspector]
        private float GetDistance()
        {
            if (_distanceSource != null)
                return _distanceSource.Distance * _deltaTime;
            return _distance;
        }

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

        [PreviewInInspector]
        public List<RaycastHit> CachedHits { get; } = new();

        public RaycastHit CachedHit => CachedHits.Count > 0 ? CachedHits[0] : default;
        public Ray CachedRay => _cachedRay;

        [Auto]
        [CompRef]
        private IRaycastProcessor _raycastProcessor;

        // [CompRef] //all in 1 就撞了？
        // [Auto]
        // private ISphereCastProcessor _sphereCastProcessor;
        // public float _sphereRadius = 0.5f; //FIXME: spherecast的半徑要怎麼處理？ 這個是用來儲存spherecast的結果
        [ShowInInspector]
        private Ray _cachedRay;

#if UNITY_EDITOR
        [ShowInDebugMode]
        private readonly List<Collider> _debugHistoryObjs = new();
#endif

        public bool _isDrawDebugColor;

        [SerializeField]
        private Color _overrideGizmoColor = Color.red;

        private void OnDrawGizmos()
        {
            if (!enabled)
                return;
            Gizmos.color = _overrideGizmoColor;
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
            // Debug.Log("[RaycastCache] Draw Gizmo Ray:" + _cachedRay, this);
            //FIXME: 要選mode? sphere cast, ray cast...
            Gizmos.DrawRay(_cachedRay.origin, _cachedRay.direction * GetDistance());
            Gizmos.DrawWireCube(_cachedRay.origin, Vector3.one * 0.1f);
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

        // [SerializeField]
        // private float _minVerticalAngle = -45f; // Minimum vertical angle limit
        //
        // [SerializeField]
        // private float _maxVerticalAngle = 45f; // Maximum vertical angle limit
        // private Transform _characterTransform; // Reference to the character's transform


        private void TryCast()
        {
            var ray = _rayProvider.GetRay();
            CachedHits.Clear();
            _cachedRay = ray;
            transform.rotation = Quaternion.LookRotation(_cachedRay.direction);
            if (_raycastMode == RaycastMode.Single)
            {
                var endPoint = _cachedRay.origin + _cachedRay.direction * GetDistance();
                if (_cacheOrigin != null)
                    _cacheOrigin.position = _cachedRay.origin;
                if (_cacheEndPoint != null)
                    _cacheEndPoint.position = endPoint;
                if (_isDrawDebugColor)
                    Debug.DrawLine(_cachedRay.origin, endPoint, _overrideGizmoColor, 10f);

                if (_raycastProcessor != null)
                {
                    if (
                        !_raycastProcessor.Raycast(
                            ray.origin,
                            ray.direction,
                            out var hitInfo,
                            GetDistance(),
                            _hittingLayer
                        )
                    )
                        return;
                    //FIXME: 操作 list好嗎？
                    CachedHits.Add(hitInfo);
                    // Debug.Log("[RaycastCache] RaycastProcessor Hit:" + hitInfo.collider, this);
                    // _thisFrameColliders.Add(hitInfo.collider);
                    _debugHistoryObjs.Add(hitInfo.collider);
                }
                else if (Physics.Raycast(ray, out var hit, GetDistance(), _hittingLayer))
                {
                    CachedHits.Add(hit);
                    _debugHistoryObjs.Add(hit.collider);
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
        private AbstractRayProvider _rayProvider;

        //update?
        // public void Simulate(float deltaTime)
        // {
        //     PhysicsUpdate();
        // }
        private float _deltaTime;

        //FIXME: raycast時間點...
        //beforeStateUpdate?
        //AfterStateUpdate?
        //怎麼保證這幾個順序？寫在StateUpdate裡一起用？

        public bool _manualUpdateMode; //FIXME: 這個要不要放在外面？ 讓外面控制

        //直接放在variable下面也是蠻好笑的？
        public void Simulate(float deltaTime) //這個優先順序問題？
        {
            // if (_manualUpdateMode)
            //     return;
            _deltaTime = deltaTime;
            TryCast();
            // Debug.Log("[RaycastCache] Simulate Ray:" + _cachedRay, this);
        }

        public void AfterUpdate() { }

        protected override string DescriptionTag => "Raycast";
        public override string Description => _rayProvider?.GetType().Name;

        public void ResetStateRestore()
        {
            //把狀態清掉
            _cachedRay = default;
            CachedHits.Clear();
            _debugHistoryObjs.Clear();
        }

        public void BeforeSimulate(float deltaTime)
        {
            // _deltaTime = deltaTime;
            // TryCast();
            // Debug.Log("[RaycastCache] BeforeSimulate Ray:" + _cachedRay, this);
        }

        public string ValueInfo => "h:" + _hittingLayer.value; //FIXME: 可能會是多個..
        public bool IsDrawingValueInfo => true;
    }

    public abstract class AbstractRayProvider : MonoBehaviour
    {
        public abstract Ray GetRay();
        //FIXME: 應該要包含距離？
    }
}

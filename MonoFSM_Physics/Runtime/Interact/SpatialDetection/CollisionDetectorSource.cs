using System.Collections.Generic;
using MonoFSM_Physics.Runtime.Interact.SpatialDetection;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.Runtime.Interact.SpatialDetection;
using MonoFSM.Variable.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSM.Core.Detection
{
    //事件要從rigidbody來QQ
    public class CollisionDetectorSource : AbstractDetectionSource
    {
        [Auto]
        [CompRef]
        private Collider _collider;

        [AutoParent]
        [SerializeField]
        private Rigidbody _rigidbody;

        [PreviewInInspector]
        [AutoParent]
        public CollisionEventListener _collisionEventListener; //用overlap? cast?

        [ShowIf("@_collisionEventListener == null")]
        [Button]
        private void AddCollisionEventListenerOnParentRigidbody()
        {
            if (_collisionEventListener == null)
                _collisionEventListener =
                    _rigidbody.gameObject.TryGetCompOrAdd<CollisionEventListener>();
        }

        protected override void Awake() //Start? 摸別人
        {
            base.Awake();
            _collisionEventListener.RegisterDetector(this);
        }

        [CompRef]
        [AutoChildren(DepthOneOnly = true)]
        private CollisionEventNode _enterNode;

        public void OnCollisionStay(Collision collision)
        {
            // 收集當前幀中仍在碰撞的collider
            _thisFrameColliders.Add(collision.collider);
        }

        public override void UpdateDetection() //FIXME: 和trigger的長得根本一樣？
        {
            // 找出新碰撞的collider (在thisFrame但不在lastFrame)
            // foreach (var col in _thisFrameColliders)
            //     if (!_lastFrameColliders.Contains(col))
            //     {
            //         Debug.Log("OnCollisionEnter", col.gameObject);
            //         QueueEnterEvent(col.gameObject);
            //         _lastCollisionEnterObj = col.gameObject;
            //     }
            //
            // // 找出離開碰撞的collider (在lastFrame但不在thisFrame)
            // foreach (var col in _lastFrameColliders)
            //     if (!_thisFrameColliders.Contains(col))
            //         QueueExitEvent(col.gameObject);

            // 更新lastFrame為thisFrame的資料
            // _lastFrameColliders.Clear();
            // _lastFrameColliders.UnionWith(_thisFrameColliders);

            // 清空thisFrame準備下一幀
            // _thisFrameColliders.Clear();

            // 處理排隊的進入/退出事件
            base.UpdateDetection();
        }

        [ShowInDebugMode]
        private GameObject _lastCollisionEnterObj;

        public override IEnumerable<DetectionResult> GetCurrentDetections()
        {
            foreach (var col in _thisFrameColliders)
                if (col != null && col.gameObject != null)
                    yield return new DetectionResult(col.gameObject);
        }
    }
}

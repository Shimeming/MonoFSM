using System.Collections.Generic;
using MonoFSM.Variable.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSM.Core.Detection
{
    public class TriggerDetector : AbstractDetectionSource
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

        private void OnTriggerStay(Collider other)
        {
            // 收集當前幀中仍在trigger內的collider
            _thisFrameColliders.Add(other);
        }

        public override void AfterDetection()
        {
            base.AfterDetection();
            _thisFrameColliders.Clear();
        }

        //FIXME: 什麼時候清掉啊
        public override void UpdateDetection()
        {
            // 找出新進入的collider (在thisFrame但不在lastFrame)
            // foreach (var col in _thisFrameColliders)
            //     if (!_lastFrameColliders.Contains(col))
            //         QueueEnterEvent(col.gameObject);
            //
            // // 找出離開的collider (在lastFrame但不在thisFrame)
            // foreach (var col in _lastFrameColliders)
            //     if (!_thisFrameColliders.Contains(col))
            //     {
            //         if (col == null)
            //         {
            //             //已經回收了，應該要先觸發事件才可以回收？
            //             Debug.LogError(
            //                 "TriggerDetector: Found a null collider in lastFrameColliders, this may cause issues.",
            //                 this
            //             );
            //         }
            //         else
            //             QueueExitEvent(col.gameObject);
            //     }
            //
            // // 更新lastFrame為thisFrame的資料
            // _lastFrameColliders.Clear();
            // _lastFrameColliders.UnionWith(_thisFrameColliders);

            // 清空thisFrame準備下一幀
            // _thisFrameColliders.Clear();

            // 處理排隊的進入/退出事件
            base.UpdateDetection();
        }

        public override IEnumerable<DetectionResult> GetCurrentDetections()
        {
            foreach (var col in _thisFrameColliders)
                if (col != null && col.gameObject != null)
                    yield return new DetectionResult(col.gameObject);
        }

        //FIXME: Gizmo?
    }
}

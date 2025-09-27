using System.Collections.Generic;
using MonoFSM.Core.Attributes;
using MonoFSM.EditorExtension;
using MonoFSM.Foundation;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSM.Core.Detection
{
    public struct DetectionResult
    {
        // public Rigidbody _targetRigidbody; //為了不要重複判斷？
        public GameObject targetObject; //collider
        public Vector3? hitPoint;
        public Vector3? hitNormal;
        public bool isValidHit;

        public DetectionResult(
            // Rigidbody targetRigidbody,
            GameObject target, //為何不用collider?
            Vector3? hitPoint = null,
            Vector3? hitNormal = null
        )
        {
            // this._targetRigidbody = targetRigidbody;
            this.targetObject = target;
            this.hitPoint = hitPoint;
            this.hitNormal = hitNormal;
            this.isValidHit = target != null;
        }

        public static DetectionResult Invalid => new DetectionResult { isValidHit = false };
    }

    //好像還是要可以拖拉耶，結構不定 FIXME: 直接和BaseDetectProcessor合併?

    // public interface IDetectionSource //AbstractComponent
    public abstract class AbstractDetectionSource
        : AbstractDescriptionBehaviour,
            IHierarchyValueInfo
    {
        protected override string DescriptionTag => "DetectionSource";

        [Required]
        [AutoParent]
        public EffectDetector _detector;

        public virtual bool IsEnabled => isActiveAndEnabled;

        //統一由 EffectDetector 管理 enter/exit 事件

        //FIXME: 要有thisFrameHitData?
        //FIXME: 已經打死是Collider了，感覺要抽象掉一層，3D間共用？傳T?
        [PreviewInInspector]
        protected readonly HashSet<Collider> _thisFrameColliders = new(); //這個有用嗎？

        [ShowInDebugMode]
        protected List<DetectionResult> _buffer = new List<DetectionResult>();

        // [PreviewInDebugMode]
        // protected readonly HashSet<Collider> _lastFrameColliders = new(); //ondisable也要清掉？

        private void OnDisable()
        {
            //FIXME: 要讓 EffectDetector handle就好？
            //TODO: Exit check?
            _thisFrameColliders.Clear();
            // _lastFrameColliders.Clear();
        }

        public abstract IEnumerable<DetectionResult> GetCurrentDetections();

        public virtual void UpdateDetection()
        {
            // DetectionSource 只負責檢測，事件處理由 EffectDetector 統一管理
        }

        public virtual void AfterDetection() { }

        /// <summary>
        ///     向 EffectDetector 報告物件進入事件
        /// </summary>
        // protected void ReportEnterEvent(
        //     GameObject obj,
        //     Vector3? hitPoint = null,
        //     Vector3? hitNormal = null
        // )
        // {
        //     this.Log("ReportEnterEvent: " + obj.name, obj);
        //     _detector.OnDetectEnterCheck(obj, hitPoint, hitNormal);
        // }
        //
        // /// <summary>
        // ///     向 EffectDetector 報告物件離開事件
        // /// </summary>
        // protected void ReportExitEvent(GameObject obj)
        // {
        //     _detector.OnDetectExitCheck(obj);
        // }

        /// <summary>
        ///     向後相容的方法，內部調用新的 ReportEnterEvent
        /// </summary>
        // protected void QueueEnterEvent(GameObject obj)
        // {
        //     ReportEnterEvent(obj);
        // }
        //
        // /// <summary>
        // ///     向後相容的方法，內部調用新的 ReportExitEvent
        // /// </summary>
        // protected void QueueExitEvent(GameObject obj)
        // {
        //     ReportExitEvent(obj);
        // }

        public string ValueInfo => "L:" + LayerMask.LayerToName(gameObject.layer);
        public bool IsDrawingValueInfo => true;
    }
}

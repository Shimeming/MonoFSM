using System.Collections.Generic;
using MonoFSM.Core.Attributes;
using MonoFSM.Foundation;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSM.Core.Detection
{
    public struct DetectionResult
    {
        public GameObject targetObject;
        public Vector3? hitPoint;
        public Vector3? hitNormal;
        public bool isValidHit;

        public DetectionResult(
            GameObject target,
            Vector3? hitPoint = null,
            Vector3? hitNormal = null
        )
        {
            this.targetObject = target;
            this.hitPoint = hitPoint;
            this.hitNormal = hitNormal;
            this.isValidHit = target != null;
        }

        public static DetectionResult Invalid => new DetectionResult { isValidHit = false };
    }

    //好像還是要可以拖拉耶，結構不定 FIXME: 直接和BaseDetectProcessor合併?

    // public interface IDetectionSource //AbstractComponent
    public abstract class IDetectionSource : AbstractDescriptionBehaviour
    {
        protected override string DescriptionTag => "DetectionSource";
        [Required] [AutoParent] public EffectDetector _detector;
        public virtual bool IsEnabled => enabled;

        //trigger類？
        [ShowInDebugMode] protected List<GameObject> _toEnter = new();

        [ShowInDebugMode] protected List<GameObject> _toExit = new();


        //FIXME: 要有thisFrameHitData?
        //FIXME: 已經打死是Collider了，感覺要抽象掉一層，3D間共用？傳T?
        [PreviewInInspector] protected readonly HashSet<Collider> _thisFrameColliders = new();

        [PreviewInInspector]
        protected readonly HashSet<Collider> _lastFrameColliders = new(); //ondisable也要清掉？

        public abstract IEnumerable<DetectionResult> GetCurrentDetections();

        public virtual void UpdateDetection()
        {
            ProcessEnterExitEvents();
        }

        protected void ProcessEnterExitEvents()
        {
            foreach (var obj in _toEnter)
            {
                var result = _detector.OnDetectEnterCheck(obj);
                // Debug.Log("OnDetectEnterCheck me: " + name + " result: " + result, this);
                // Debug.Log("OnDetectEnterCheck other: " + obj.name + " result: " + result, obj);
            }

            _toEnter.Clear();

            foreach (var obj in _toExit) _detector.OnDetectExitCheck(obj);

            _toExit.Clear();
        }

        protected void QueueEnterEvent(GameObject obj) //fixme 現在沒有管 hit data那些耶
        {
            // Debug.Log("QueueEnterEvent: " + obj.name, obj);
            _toEnter.Add(obj);
        }

        protected void QueueExitEvent(GameObject obj)
        {
            _toExit.Add(obj);
        }
    }
}

using System.Collections.Generic;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.Simulate;
using MonoFSM.Foundation;
using MonoFSM.Runtime.Interact.EffectHit;
using MonoFSM.Variable.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSM.Core.Detection
{
    // [Serializable]
    public struct DetectData
    {
        private EffectDetector _detector;
        private BaseEffectDetectTarget _detectable;

        //清掉
        public DetectData(EffectDetector detector, BaseEffectDetectTarget detectable)
        {
            _detector = detector;
            _detectable = detectable;
            _isCustomHitPoint = false; //預設不是自定義hitPoint
            _hitPoint = Vector3.zero; //預設hitPoint為零
            _hitNormal = Vector3.zero; //預設hitNormal為零
        }

        public void SetCustomHitPoint(Vector3 point)
        {
            _isCustomHitPoint = true;
            _hitPoint = point;
        }

        public void SetCustomNormal(Vector3 normal)
        {
            _isCustomHitPoint = true; //這個是hitPoint的normal
            _hitNormal = normal;
        }

        private bool _isCustomHitPoint;
        private Vector3 _hitPoint;
        private Vector3 _hitNormal;

        public Vector3 hitPoint => _isCustomHitPoint ? _hitPoint : _detectable.transform.position;
        public Vector3 hitNormal => _isCustomHitPoint ? _hitNormal : -_detector.transform.forward;
    }

    [DisallowMultipleComponent]
    public class EffectDetector : AbstractDescriptionBehaviour, IDefaultSerializable,
        IUpdateSimulate
    {
        [SerializeField] private string _designName;
        public override string Description => _designName;

        [CompRef]
        [AutoChildren(DepthOneOnly = true)]
        private AbstractConditionBehaviour[] _conditions;

        public bool IsValid => _conditions.IsAllValid();

        //FIXME: 多個會無法分辨誰造成的
        [Required]
        [CompRef]
        [AutoChildren(DepthOneOnly = true)]
        private IDetectionSource[] _detectionSources; //想要手動拉？不好？還是裝下面嗎？

        private readonly List<EffectDetectable> _toRemove = new();
        private readonly Dictionary<GameObject, EffectDetectable> _currentDetections = new();

        //FIXME: Receiver的部分要怎麼處理？ 也會有開關的問題？還是沒差遇到再說
        private void OnDisable()
        {
            if (!Application.isPlaying)
                return;
            // Debug.Log("OnDisable of detector",this);
            //copy _detectedObjects to toRemove
            _toRemove.AddRange(_detectedObjects);
            foreach (var detectable in _toRemove)
                // Debug.Log("OnDisable of detectable",detectable);
                OnDetectExitCheck(detectable.gameObject);

            _toRemove.Clear();
            _detectedObjects.Clear();
            // OnDisableImplement();
        }

        // protected abstract void OnDisableImplement();

        // [AutoParent] private StateMachineOwner owner;
        // public StateMachineOwner Owner => owner;
        [CompRef]
        [AutoChildren]
        private GeneralEffectDealer[] dealers;

        //GameObject必定要在Detector的layer
        // [FormerlySerializedAs("hittingLayer")]
        // [CustomSerializable]
        // [ShowInInspector]
        // // [OnValueChanged(nameof(SetLayerOverride))]
        // [Required]
        // public LayerMask HittingLayer;
        // protected abstract void SetLayerOverride();

        [PreviewInInspector]
        protected HashSet<EffectDetectable> _detectedObjects = new();
#if UNITY_EDITOR
        // [PreviewInInspector] private List<EffectDetectable> currentDetectedObjects => _detectedObjects.ToList();
        [PreviewInInspector]
        protected HashSet<EffectDetectable> _lastDetectedObjects = new();

        [Button]
        private void ClearLastDetectedObjects()
        {
            _lastDetectedObjects.Clear();
        }
#endif

        // protected abstract void AssignHitPoint(DetectData data);
        //FIXME: 這個是spatial Detector的特性，不是所有的Detector都有

        //fixme:可以有直接傳過來的版本？
        //FIXME: return (bool & string)
        public string OnDetectEnterCheck(
            GameObject other,
            Vector3? point = null,
            Vector3? normal = null
        ) //可能需要帶其他額外參數？像是collision的資訊
        {
            if (IsValid == false) //條件不符合
                return "Detector is not valid";
            //理論上不該打到別的東西，layer就擋掉了才對 (有分layer的話)

            //FIXME: 往parent找？ gc問題？

            if (other.TryGetComponent<BaseEffectDetectTarget>(out var spatialDetectable))
            {
                // Debug.LogError(other.name + " is not a EffectDetectable" + other.gameObject.layer,
                //     other);
                // return "not a BaseEffectDetectTarget";
            }
            else
            {
                spatialDetectable = other.GetComponentInParent<BaseEffectDetectTarget>();
            }

            if (spatialDetectable == null)
            {
                // Debug.LogError(
                //     other.name + " is not a BaseEffectDetectTarget" + other.gameObject.layer,
                //     other);
                return "not a BaseEffectDetectTarget";
            }

            // Debug.Log("OnSpatialEnter: " + spatialDetectable.name + " by " + gameObject.name, this);
            var detectData = new DetectData(this, spatialDetectable);

            if (point != null)
                detectData.SetCustomHitPoint(point.Value);
            if (normal != null)
                detectData.SetCustomNormal(normal.Value);

            //FIXME: 物理的想要繞掉，另外做condition?
            // if (spatialDetectable.Owner == Owner) return; //自己身上的不算
            _detectedObjects.Add(spatialDetectable.Detectable);
#if UNITY_EDITOR
            _lastDetectedObjects.Add(spatialDetectable.Detectable);
            spatialDetectable.Detectable._detectors.Add(this); //FIXME: 有點醜？
#endif
            // Debug.Log("OnSpatialEnter dealers:"+dealers.Length+" receivers:"+effectCollider.EffectReceivers.Length, this);
            //FIXME: 用update撈起來等等再判？
            if (dealers == null)
            {
                Debug.LogError("Dealers is null", this);
                return "Dealers is null";
            }

            foreach (var dealer in dealers)
            {
                if (!dealer.IsValid)
                {
                    dealer.SetFailReason("Dealer is not valid");
                    continue;
                }

                foreach (var receiver in spatialDetectable.EffectReceivers) //condition會錯？因為一直打？
                {
                    //FIXME: proxy的判定
                    if (!dealer.CanHitReceiver(receiver))
                        continue; //不會打到的不算
                    //移到System?
                    //互動雙方的條件描述
                    //每個receiver都一個？多餘嗎？
                    var hitData = receiver.GenerateEffectHitData(dealer);

                    hitData.hitNormal = normal;
                    hitData.hitPoint = point;

                    if (point != null)
                        Debug.Log("HitPoint is set to: " + hitData.hitPoint + point, this);
                    if (normal != null)
                        Debug.Log("HitNormal is set to: " + hitData.hitNormal, this);

                    dealer.OnHitEnter(hitData, detectData);
                    receiver.OnEffectHitEnter(hitData, detectData);
                }
            }
            return "Detection successful";
        }

        public void OnDetectExitCheck(GameObject other)
        {
            if (!other.TryGetComponent<TriggerDetectableTarget>(out var triggerDetectableTarget))
                // Debug.LogError(other.name + " is not a GeneralEffectCollider" + other.gameObject.layer);
                return;

            var spatialDetectable = triggerDetectableTarget.Detectable;

            _detectedObjects.Remove(spatialDetectable);
#if UNITY_EDITOR
            spatialDetectable._detectors.Remove(this);
#endif
            //FIXME: 連點會有狀態問題耶...
            if (dealers != null)
                foreach (var dealer in dealers)
                //FIXME: 點下去，可能就造成dealer的condition變了耶
                // if (!dealer.IsValid) //有點討厭，這個很容易漏掉, 這個會讓
                // {
                //     dealer.Log("Dealer is not valid");
                //     continue;
                // }
                //Dealer觸發後，造成條件變化了，這樣這邊會很難判定？
                foreach (var receiver in spatialDetectable.EffectReceivers)
                {
                    //對稱
                    if (!dealer.IsEnteredReceiver(receiver))
                        continue;
                    // if (!dealer.CanHitReceiver(receiver)) continue;
                    //FIXME: 這個是不是不該generate? 還是重新gen也還好
                    var hitData = receiver.GenerateEffectHitData(dealer);

                    dealer.OnHitExit(hitData);
                    receiver.OnEffectHitExit(hitData);
                }
        }

        public void Simulate(float deltaTime)
        {
            if (!IsValid || _detectionSources == null)
                return;

            // var previousDetections = new Dictionary<GameObject, EffectDetectable>(
            //     _currentDetections
            // );
            // _currentDetections.Clear();

            foreach (var detectionSource in _detectionSources)
            {
                if (!detectionSource.IsEnabled)
                    continue;
                detectionSource.UpdateDetection();
            }

            //FIXME: 蛤？
            // foreach (var kvp in previousDetections)
            // {
            //     if (!_currentDetections.ContainsKey(kvp.Key))
            //     {
            //         OnDetectExitCheck(kvp.Key);
            //     }
            // }
        }

        public void AfterUpdate() { }
        protected override string DescriptionTag => "Detector";
    }
}

using System.Collections.Generic;
using _1_MonoFSM_Core.Runtime.EffectHit.Action;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.Simulate;
using MonoFSM.CustomAttributes;
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
        private readonly EffectDetectable _detectable;

        //清掉
        public DetectData(EffectDetector detector, EffectDetectable detectable)
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
    public class EffectDetector
        : AbstractDescriptionBehaviour,
            IDefaultSerializable,
            IUpdateSimulate,
            IDropdownRoot
    {
        //FIXME: 這個不好...會以為可以改name結果又跑掉？
        [SerializeField]
        private string _designName;

        public override string Description => ReformatedName;

        [CompRef]
        [AutoChildren(DepthOneOnly = true)]
        private AbstractConditionBehaviour[] _conditions;

        public bool IsValid => _conditions.IsAllValid();

        //FIXME: 多個會無法分辨誰造成的
        [Required]
        [CompRef]
        [AutoChildren(DepthOneOnly = true)]
        private IDetectionSource[] _detectionSources; //想要手動拉？不好？還是裝下面嗎？

        [Required]
        [CompRef]
        [AutoChildren(DepthOneOnly = true)]
        private IDetectionSource _detectionSource;

        private readonly List<EffectDetectable> _toRemove = new();
        private readonly Dictionary<GameObject, EffectDetectable> _currentDetections = new();
        private readonly HashSet<EffectDetectable> _processedThisFrame = new();

        // 追蹤 dealer 狀態以檢測變化
        private readonly Dictionary<GeneralEffectDealer, bool> _dealerLastStates = new();

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

        [RequiredListLength(MinLength = 1)]
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

            var detectable = GetEffectDetectable(other);
            if (detectable == null)
            {
                return "not a EffectDetectable";
            }

            // 檢查這個EffectDetectable是否已經在這個frame處理過了
            if (!_processedThisFrame.Add(detectable))
                return "EffectDetectable already processed this frame";

            // 標記為已處理

            var detectData = new DetectData(this, detectable);

            if (point != null)
                detectData.SetCustomHitPoint(point.Value);
            if (normal != null)
                detectData.SetCustomNormal(normal.Value);

            //FIXME: 物理的想要繞掉，另外做condition?
            // if (spatialDetectable.Owner == Owner) return; //自己身上的不算
            _detectedObjects.Add(detectable);
#if UNITY_EDITOR
            _lastDetectedObjects.Add(detectable);
            detectable._debugDetectors.Add(this); //FIXME: 有點醜？
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

                //FIXME: receiver有可能同個frame已經處理過了，不同的物理進入點？
                foreach (var receiver in detectable.EffectReceivers) //condition會錯？因為一直打？
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
            var detectable = GetEffectDetectable(other);
            if (detectable == null)
                return;

            _detectedObjects.Remove(detectable);
#if UNITY_EDITOR
            detectable._debugDetectors.Remove(this);
#endif

            if (dealers != null)
            {
                foreach (var dealer in dealers)
                {
                    foreach (var receiver in detectable.EffectReceivers)
                    {
                        if (!dealer.IsEnteredReceiver(receiver))
                            continue;

                        var hitData = receiver.GenerateEffectHitData(dealer);
                        dealer.OnHitExit(hitData);
                        receiver.OnEffectHitExit(hitData);
                    }
                }
            }
        }

        //需要debug是誰改的嗎？
        public ManualEffectDetectAction _manualEffectDetectAction; //被Action控走的話，就不自己update了

        //FIXME: manual Update? from StateAction?
        public void Simulate(float deltaTime)
        {
            if (!IsValid || _detectionSources == null || _manualEffectDetectAction != null)
                return;
            DetectCheck();
        }

        public void DetectCheck() //關掉就沒檢查了...不就導致沒辦法判斷exit了嗎？ 最後還是要OnDisable處理喔？
        {
            // 每frame開始時清空已處理的集合
            _processedThisFrame.Clear();

            // 1. 檢查 dealer 狀態變化，如果有變化才重新評估
            if (CheckDealerStateChanges())
                ReevaluateExistingDetections();

            // 2. 讓 DetectionSource 更新空間檢測（現有邏輯）
            foreach (var detectionSource in _detectionSources)
            {
                if (!detectionSource.IsEnabled)
                    continue;
                detectionSource.UpdateDetection(); // 這會呼叫 OnDetectEnterCheck
            }
        }

        private void ReevaluateExistingDetections()
        {
            // Debug.Log("Reevaluating existing detections due to dealer state changes", this);

            // 先收集所有狀態變化
            var dealerStateChanges =
                new Dictionary<GeneralEffectDealer, (bool lastState, bool currentState)>();

            foreach (var dealer in dealers)
            {
                var currentState = dealer.IsValid;
                var lastState = _dealerLastStates.GetValueOrDefault(dealer, false);
                if (currentState != lastState)
                    dealerStateChanges[dealer] = (lastState, currentState);
            }

            // 對每個 detectable 處理狀態變化
            foreach (var detectable in _detectedObjects)
            {
                // 檢查是否這個 frame 已經處理過了（避免和 DetectionSource 重複）
                if (!_processedThisFrame.Add(detectable))
                    continue;

                // 處理狀態變化
                ProcessDealerStateChangesForDetectable(detectable, dealerStateChanges);
            }

            // 最後統一更新狀態記錄
            foreach (var kvp in dealerStateChanges)
                _dealerLastStates[kvp.Key] = kvp.Value.currentState;
        }

        private void ProcessDealerStateChangesForDetectable(
            EffectDetectable detectable,
            Dictionary<GeneralEffectDealer, (bool lastState, bool currentState)> dealerStateChanges
        )
        {
            // Debug.Log("Processing dealer state changes for detectable", detectable);

            foreach (var kvp in dealerStateChanges)
            {
                var dealer = kvp.Key;
                var currentState = kvp.Value.currentState;

                if (currentState)
                {
                    // dealer 剛變有效，觸發 enter 事件
                    // Debug.Log("Dealer state changed to valid, triggering enter event", dealer);
                    TriggerEnterForDealerAndDetectable(dealer, detectable);
                }
                else
                {
                    // dealer 剛變無效，觸發 exit 事件
                    TriggerExitForDealerAndDetectable(dealer, detectable);
                }
            }
        }

        private void TriggerEnterForDealerAndDetectable(
            GeneralEffectDealer dealer,
            EffectDetectable detectable
        )
        {
            var detectData = new DetectData(this, detectable);

            foreach (var receiver in detectable.EffectReceivers)
            {
                if (!dealer.CanHitReceiver(receiver))
                    continue;

                var hitData = receiver.GenerateEffectHitData(dealer);
                dealer.OnHitEnter(hitData, detectData);
                receiver.OnEffectHitEnter(hitData, detectData);
            }
        }

        private void TriggerExitForDealerAndDetectable(
            GeneralEffectDealer dealer,
            EffectDetectable detectable
        )
        {
            foreach (var receiver in detectable.EffectReceivers)
            {
                if (!dealer.IsEnteredReceiver(receiver))
                    continue;

                var hitData = receiver.GenerateEffectHitData(dealer);
                dealer.OnHitExit(hitData);
                receiver.OnEffectHitExit(hitData);
            }
        }

        public void AfterUpdate() { }

        private bool CheckDealerStateChanges()
        {
            if (dealers == null)
                return false;

            var hasChanges = false;

            foreach (var dealer in dealers)
            {
                var currentState = dealer.IsValid;
                var lastState = _dealerLastStates.GetValueOrDefault(dealer, false);

                if (currentState != lastState)
                    hasChanges = true;
                // 不在這裡更新狀態，留給 ProcessDealerStateChangesForDetectable 處理後再更新
            }

            return hasChanges;
        }

        private EffectDetectable GetEffectDetectable(GameObject target)
        {
            // 先嘗試直接取得 EffectDetectable
            if (target.TryGetComponent(out EffectDetectable detectable))
                return detectable;

            // 透過 BaseEffectDetectTarget 取得
            var spatialDetectable = target.GetComponentInParent<BaseEffectDetectTarget>();
            if (spatialDetectable != null)
                return spatialDetectable.Detectable;

            // 透過 TriggerDetectableTarget 取得 (向後相容)
            if (target.TryGetComponent<TriggerDetectableTarget>(out var triggerDetectable))
                return triggerDetectable.Detectable;

            return null;
        }

        protected override string DescriptionTag => "Detector";
    }
}

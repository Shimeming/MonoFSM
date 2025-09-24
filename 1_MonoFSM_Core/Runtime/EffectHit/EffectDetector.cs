using System.Collections.Generic;
using _1_MonoFSM_Core.Runtime.EffectHit.Action;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.Simulate;
using MonoFSM.CustomAttributes;
using MonoFSM.Foundation;
using MonoFSM.Runtime.Interact.EffectHit;
using MonoFSM.Variable.Attributes;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace MonoFSM.Core.Detection
{
    // [Serializable]
    public struct DetectData //分兩種，好像多餘？
    {
        private EffectDetector _detector; //需要知道這個嗎？用dealer/receiver就好？
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

        public EffectDetectable detectable => _detectable;
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

        [ShowInDebugMode]
        public bool IsValid => _conditions.IsAllValid();

        //FIXME: 多個會無法分辨誰造成的
        [Required]
        [CompRef]
        [AutoChildren(DepthOneOnly = true)]
        private AbstractDetectionSource[] _detectionSources; //想要手動拉？不好？還是裝下面嗎？

        // [Required]
        // [CompRef]
        // [AutoChildren(DepthOneOnly = true)]
        // private AbstractDetectionSource _detectionSource;

        private readonly List<EffectDetectable> _toRemove = new(); // 用於 OnDisable 清理

        // 追蹤 dealer 狀態以檢測變化
        private readonly Dictionary<GeneralEffectDealer, bool> _dealerLastStates = new();

        //FIXME: Receiver的部分要怎麼處理？ 也會有開關的問題？還是沒差遇到再說
        private void OnDisable()
        {
            if (!Application.isPlaying)
                return;
            // Debug.Log("OnDisable of detector",this);
            //copy _detectedObjects to toRemove
            _toRemove.AddRange(_thisFrameDetectedObjects.Keys);
            foreach (var detectable in _toRemove)
                // Debug.Log("OnDisable of detectable",detectable);
                TriggerExitEventsForDetectable(detectable);

            _toRemove.Clear();
            _thisFrameDetectedObjects.Clear();

            // OnDisableImplement();
        }

        [RequiredListLength(MinLength = 1)]
        [CompRef]
        [AutoChildren]
        private GeneralEffectDealer[] _dealers;

        public GeneralEffectDealer[] Dealers => _dealers;

        //GameObject必定要在Detector的layer
        // [FormerlySerializedAs("hittingLayer")]
        // [CustomSerializable]
        // [ShowInInspector]
        // // [OnValueChanged(nameof(SetLayerOverride))]
        // [Required]
        // public LayerMask HittingLayer;
        // protected abstract void SetLayerOverride();

        [PreviewInInspector]
        protected Dictionary<EffectDetectable, DetectData> _thisFrameDetectedObjects = new();
#if UNITY_EDITOR
        // [PreviewInInspector] private List<EffectDetectable> currentDetectedObjects => _detectedObjects.ToList();
        [PreviewInInspector]
        protected Dictionary<EffectDetectable, DetectData> _lastDetectedObjects = new();

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
        /// <summary>
        /// 向後相容方法 - 現在由 DetectCheck 統一管理，此方法僅作調試用途
        /// </summary>
        [System.Obsolete(
            "Use DetectCheck() instead. This method is for backward compatibility only."
        )]
        //不該從這裡？
        public string OnDetectEnterCheck(
            GameObject other,
            Vector3? point = null, //FIXME: 一定要給？
            Vector3? normal = null
        )
        {
            // 向後相容：仍然支援手動觸發，但建議使用新的統一管理機制
            if (IsValid == false)
                return "Detector is not valid";

            var detectable = GetEffectDetectable(other);
            if (detectable == null)
                return "not a EffectDetectable";

            // 手動加入到檢測列表（用於向後相容）
            var detectData = new DetectData(this, detectable);
            if (point != null)
                detectData.SetCustomHitPoint(point.Value);
            if (normal != null)
                detectData.SetCustomNormal(normal.Value);
            _thisFrameDetectedObjects[detectable] = detectData;

#if UNITY_EDITOR
            _lastDetectedObjects[detectable] = detectData;
            detectable._debugDetectors.Add(this);
#endif

            // 直接觸發進入事件
            TriggerEnterEventsForDetectable(detectData);
            return "Detection successful";
        }

        /// <summary>
        /// 向後相容方法 - 現在由 DetectCheck 統一管理，此方法僅作調試用途
        /// </summary>
        [System.Obsolete(
            "Use DetectCheck() instead. This method is for backward compatibility only."
        )]
        public void OnDetectExitCheck(GameObject other)
        {
            var detectable = GetEffectDetectable(other);
            if (detectable == null)
                return;

            // 手動從檢測列表移除（用於向後相容）
            _thisFrameDetectedObjects.Remove(detectable);

#if UNITY_EDITOR
            detectable._debugDetectors.Remove(this);
#endif

            // 直接觸發離開事件
            TriggerExitEventsForDetectable(detectable);
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

        [ShowInDebugMode]
        float _lastDetectCheckTime = 0f;

        public void DetectCheck()
        {
            // 每frame重建檢測列表
            _lastDetectCheckTime = Time.time;
            // 1. 記錄上一幀的檢測狀態
            _lastDetectedObjects.Clear();
            foreach (var kvp in _thisFrameDetectedObjects)
                _lastDetectedObjects[kvp.Key] = kvp.Value;
            // var previousDetected = new HashSet<EffectDetectable>(_detectedObjects);

            // 2. 清空當前檢測列表，準備重建
            _thisFrameDetectedObjects.Clear();

            // 3. 收集所有 DetectionSource 的當前檢測結果
            foreach (var detectionSource in _detectionSources)
            {
                if (!detectionSource.isActiveAndEnabled)
                {
                    detectionSource.AfterDetection();
                    continue;
                }

                // 讓 DetectionSource 更新其內部狀態
                detectionSource.UpdateDetection();

                // 收集當前檢測到的物件
                foreach (var result in detectionSource.GetCurrentDetections())
                {
                    if (result.isValidHit)
                    {
                        var detectable = GetEffectDetectable(result.targetObject);
                        if (detectable != null && IsValid)
                        {
                            var detectData = new DetectData(this, detectable);
                            if (result.hitPoint.HasValue)
                                detectData.SetCustomHitPoint(result.hitPoint.Value);
                            if (result.hitNormal.HasValue)
                                detectData.SetCustomNormal(result.hitNormal.Value);
                            _thisFrameDetectedObjects[detectable] = detectData;
                        }
                    }
                }

                //放這OK嗎？ 小心上面的foreach?
                detectionSource.AfterDetection();
            }

            // 4. 檢查 dealer 狀態變化
            if (CheckDealerStateChanges())
                HandleDealerStateChanges();

            // 5. 比較前後差異，觸發 Enter/Exit 事件
            ProcessDetectionChanges(_lastDetectedObjects, _thisFrameDetectedObjects);
        }

        private void HandleDealerStateChanges()
        {
            // 先收集所有狀態變化
            var dealerStateChanges =
                new Dictionary<GeneralEffectDealer, (bool lastState, bool currentState)>();

            foreach (var dealer in _dealers)
            {
                var currentState = dealer.IsValid;
                var lastState = _dealerLastStates.GetValueOrDefault(dealer, false);
                if (currentState != lastState)
                    dealerStateChanges[dealer] = (lastState, currentState);
            }

            // 對每個當前檢測到的 detectable 處理狀態變化
            foreach (var kvp in _thisFrameDetectedObjects)
            {
                ProcessDealerStateChangesForDetectable(kvp.Key, dealerStateChanges);
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

            // foreach (var receiver in detectable.EffectReceivers)
            // {
            //
            // }
            var receiver = detectable.Get(dealer._effectType);
            if (!dealer.CanHitReceiver(receiver))
                return;

            var hitData = receiver.GenerateEffectHitData(dealer);
            dealer.OnHitEnter(hitData, detectData);
            receiver.OnEffectHitEnter(hitData, detectData);
        }

        private void TriggerExitForDealerAndDetectable(
            GeneralEffectDealer dealer,
            EffectDetectable detectable
        )
        {
            // foreach (var receiver in detectable.EffectReceivers)
            // {
            //
            // }
            var receiver = detectable.Get(dealer._effectType);
            if (!dealer.IsEnteredReceiver(receiver))
                return;

            var hitData = receiver.GenerateEffectHitData(dealer);
            dealer.OnHitExit(hitData);
            receiver.OnEffectHitExit(hitData);
        }

        public void AfterUpdate() { }

        //通常物理的進入點
        private void ProcessDetectionChanges(
            Dictionary<EffectDetectable, DetectData> previousDetected,
            Dictionary<EffectDetectable, DetectData> currentDetected
        )
        {
            // 找出新進入的物件（在current但不在previous）
            foreach (var kvp in currentDetected)
            {
                var detectable = kvp.Key;
                var detectData = kvp.Value;
                if (!previousDetected.ContainsKey(detectable))
                {
                    TriggerEnterEventsForDetectable(detectData);
#if UNITY_EDITOR
                    _lastDetectedObjects[detectable] = detectData;
                    detectable._debugDetectors.Add(this);
#endif
                }
            }

            // 找出離開的物件（在previous但不在current）
            foreach (var kvp in previousDetected)
            {
                var detectable = kvp.Key;
                if (!currentDetected.ContainsKey(detectable))
                {
                    TriggerExitEventsForDetectable(detectable);
#if UNITY_EDITOR
                    detectable._debugDetectors.Remove(this);
#endif
                }
            }
        }

        private void TriggerEnterEventsForDetectable(DetectData detectData)
        {
            if (_dealers == null)
            {
                Debug.LogError("Dealers is null", this);
                return;
            }

            this.Log($"TriggerEnterEventsForDetectable: {detectData.detectable.name}");
            foreach (var dealer in _dealers)
            {
                if (!dealer.IsValid)
                {
                    dealer.SetFailReason("Dealer is not valid");
                    continue;
                }

                this.Log($"Processing dealer: {dealer.name}");
                var receiver = detectData.detectable.Get(dealer._effectType);
                if (!dealer.CanHitReceiver(receiver))
                    continue;

                var hitData = receiver.GenerateEffectHitData(dealer);
                hitData.hitNormal = detectData.hitNormal;
                hitData.hitPoint = detectData.hitPoint;

                dealer.OnHitEnter(hitData, detectData);
                receiver.OnEffectHitEnter(hitData, detectData);
            }
        }

        private void TriggerExitEventsForDetectable(EffectDetectable detectable)
        {
            if (_dealers == null)
                return;

            foreach (var dealer in _dealers)
            {
                var receiver = detectable.Get(dealer._effectType);

                // foreach (var receiver in detectable.EffectReceivers)
                // {
                //
                // }
                if (!dealer.IsEnteredReceiver(receiver))
                    continue;

                var hitData = receiver.GenerateEffectHitData(dealer);
                dealer.OnHitExit(hitData);
                receiver.OnEffectHitExit(hitData);
            }
        }

        private bool CheckDealerStateChanges()
        {
            if (_dealers == null)
                return false;

            var hasChanges = false;

            foreach (var dealer in _dealers)
            {
                var currentState = dealer.IsValid;
                var lastState = _dealerLastStates.GetValueOrDefault(dealer, false);

                if (currentState != lastState)
                    hasChanges = true;
                // 不在這裡更新狀態，留給 HandleDealerStateChanges 處理後再更新
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

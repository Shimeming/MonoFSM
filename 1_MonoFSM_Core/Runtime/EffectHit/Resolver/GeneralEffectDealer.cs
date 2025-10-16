using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.DataProvider;
using MonoFSM.Core.Detection;
using MonoFSM.Variable.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace MonoFSM.Runtime.Interact.EffectHit
{
    public class ProxySource { }

    //FIXME: 篩選掉同個owner下的判斷？

    public class GeneralEffectDealer : EffectResolver, IEffectDealer
    {
        // public VariableMonoDescriptableProvider proxyProvider;
        // public GeneralEffectType effectType;
        [Header("自動找EffectType相同的Dealer")] //[SerializeReference]
        [Auto]
        // [PreviewInInspector]
        [Component]
        // [ShowDrawerChain]
        private IVarMonoProvider _proxyProvider;

        [PreviewInInspector]
        private GeneralEffectDealer proxyDealer => _proxyProvider?.Value?.GetDealer(_effectType);

        //互動時，兩個都可以執行耶，那EffectHitData怎麼算呢？ ex: 人dealer耗體力，斧頭dealer耗耐久


        //FIXME: 要必須有嗎？如果null就表示可以當純偵測器...
        // [PropertyOrder(-1)]
        // public FloatValueSource ValueSource;

        // [Auto]
        // // [PreviewInInspector]
        // [Component]
        // [PropertyOrder(-1)]
        // private IFloatProvider _valueSource; //FIXME: 還是要把情境也寫死？
        //FIXME: 可能還會涉及多個varfloat,不一定需要？ 用getFloat就好了
        //通常就是 A 打 B
        //A有value
        //B有cost
        //或甚至有整套判定+運算，ApplyEffectCondition, ApplyEffects

        [PreviewInInspector]
        [AutoParent]
        private IBinder _binder;

        public bool IsEnteredReceiver(IEffectReceiver receiver)
        {
            return _receivers.Contains(receiver);
        }

        [ShowInDebugMode]
        private string _failReason = "No Fail Reason";

        [Conditional("UNITY_EDITOR")]
        public void SetFailReason(string reason)
        {
            _failReason = reason;
        }

        public bool CanHitReceiver(IEffectReceiver receiver)
        {
            if (receiver == null)
            {
                SetFailReason("Receiver is null");
                return false;
            }
            // if (!IsValid)
            // {
            //     return false;
            // }
            SetFailReason("Check");

            var r = (GeneralEffectReceiver)receiver;
            if (r._effectType != _effectType)
            {
                _candidateReceivers.Add(receiver); //什麼時候清掉？
                SetFailReason("EffectType mismatch");
                return false;
            }

            if (!receiver.IsValid) //沒開的不算
            {
                _candidateReceivers.Add(receiver); //什麼時候清掉？
                SetFailReason("Receiver is not valid");
                return false;
            }

            if (_proxyProvider != null) //指定需要透過ProxyProvider拿 ex: 斧頭上的Dealer
            {
                if (proxyDealer == null) //並沒有找到Proxy Dealer，失敗
                {
                    SetFailReason("ProxyDealer is null");
                    var data = r.GenerateEffectHitData(this);
                    OnEffectHitConditionFail(data);
                    r.OnEffectHitConditionFail(data);
                    return false;
                }

                proxyDealer.CanHitReceiver(r); //繼續判囉？
            }

            if (_effectConditions != null)
                foreach (var condition in _effectConditions)
                {
                    var result = condition.IsEffectHitValid(r);
                    if (!result)
                    {
                        SetFailReason($"EffectCondition {condition.GetType().Name} failed");
                        var data = r.GenerateEffectHitData(this);
                        OnEffectHitConditionFail(data);
                        r.OnEffectHitConditionFail(data);
                        return false;
                    }
                }

            if (!r.IsEffectConditionsAllValid(this))
            {
                SetFailReason($"Receiver's EffectCondition fail");
                var data = r.GenerateEffectHitData(this);
                OnEffectHitConditionFail(data);
                r.OnEffectHitConditionFail(data);
                return false;
            }

#if UNITY_EDITOR
            this.Log("HitReceiver Success:"); //, r.GetGlobalId());
#endif
            SetFailReason("HitReceiver Success");
            return true;
        }

        // public float FinalValue => _valueSource.Value;

        //FIXME: runtime receivers
        [PreviewInInspector]
        private HashSet<GeneralEffectReceiver> _receivers = new();

        [Header("Condition不符合的")]
        [PreviewInDebugMode]
        private HashSet<IEffectReceiver> _candidateReceivers = new();

        [PreviewInInspector]
        private GeneralEffectReceiver _lastReceiver;

        GeneralEffectReceiver _lastBestMatchReceiver;
        public GeneralEffectReceiver BestMatchReceiver => _lastBestMatchReceiver;

        public void OnBestMatchCheck()
        {
            if (_onlyTriggerBestMatch != null)
            {
                // Debug.Log("OnBestMatchCheck", this);
                if (_receivers.Count == 0)
                {
                    if (_lastBestMatchReceiver != null)
                        _lastBestMatchReceiver.OnEffectHitBestMatchExit(_currentHitData);
                    _lastBestMatchReceiver = null;
                    return;
                }

                if (_receivers.Count == 1)
                {
                    var only = _receivers.First();
                    if (_lastBestMatchReceiver != only)
                    {
                        if (_lastBestMatchReceiver != null)
                            _lastBestMatchReceiver.OnEffectHitBestMatchExit(_currentHitData);
                        only.OnEffectHitBestMatchEnter(_currentHitData);
                        // Debug.Log($"Only one receiver, {only.name} is the best match", this);
                        _lastBestMatchReceiver = only;
                    }

                    return;
                }

                GeneralEffectReceiver bestMatch = null;
                float bestScore = float.MinValue;
                foreach (var receiver in _receivers)
                {
                    var score = _onlyTriggerBestMatch.CalculateScore(this, receiver);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMatch = receiver;
                    }
                }

                if (_lastBestMatchReceiver != bestMatch)
                {
                    if (_lastBestMatchReceiver != null)
                        _lastBestMatchReceiver.OnEffectHitBestMatchExit(_currentHitData);
                    if (bestMatch != null)
                        bestMatch.OnEffectHitBestMatchEnter(_currentHitData);
                    _lastBestMatchReceiver = bestMatch;
                }
            }
        }

        public void OnHitEnter(IEffectHitData data, DetectData? detectData = null)
        {
            _currentHitData = data as GeneralEffectHitData;
            if (_currentHitData == null)
            {
                Debug.LogError("EffectHitData is not GeneralEffectHitData");
                return;
            }
            if (_proxyProvider != null)
                proxyDealer.OnHitEnter(_currentHitData, detectData);

            var receiverEntity = _currentHitData.GeneralReceiver.ParentEntity;
            _enterNode?._hittingEntity?.SetValue(receiverEntity, this); //要先做
            _enterNode?.EventHandle(_currentHitData);

            _receivers.Add(_currentHitData.GeneralReceiver);
            _hittingEntities.Add(receiverEntity);

            _lastReceiver = data.Receiver as GeneralEffectReceiver;
        }

        private readonly HashSet<MonoEntity> _hittingEntities = new();

        public List<MonoEntity> GetHittingEntities()
        {
            return _hittingEntities.ToList();
        }

        public void OnHitExit(IEffectHitData data)
        {
            var hitData = data as GeneralEffectHitData;
            //_receivers裡面要有才可以做這件事
            if (_proxyProvider != null)
                proxyDealer.OnHitEnter(data);

            _exitNode?.EventHandle(data as GeneralEffectHitData);
            _receivers.Remove((GeneralEffectReceiver)data.Receiver);
            _hittingEntities.Remove(hitData.GeneralReceiver.ParentEntity);
        }

        protected override string TypeTag => "Dealer";
        protected override string DescriptionTag => "Dealer";

        [CompRef]
        [Auto]
        private AbstractOnlyTriggerBestMatch _onlyTriggerBestMatch;

        // public AbstractOnlyTriggerBestMatch OnlyTriggerBestMatch => _onlyTriggerBestMatch;
        public bool IsOnlyTriggerBestMatch => _onlyTriggerBestMatch != null;
    }
}

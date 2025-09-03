using System.Collections.Generic;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.Detection;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSM.Runtime.Interact.EffectHit
{
    //FIXME: 應該要怎麼轉接比較好，我會有好幾種事件類型，幫每種事件類型定義類別，再讓下面的action去做事
    public class GeneralEffectReceiver : EffectResolver, IEffectReceiver, IDetectDataProvider
    {
        private void OnValidate()
        {
            transform.localPosition = Vector3.zero;
        }

        [Component(AddComponentAt.Parent)]
        [Required]
        [AutoParent]
        private EffectDetectable _detectable; //不一定是，IEffectDetectable?

        // [PropertyOrder(-1)]
        // public  ValueSource; //FIXME: 拿來做什麼？

        //FIXME: 從GeneralEffectHitData？
        public GeneralEffectHitData GenerateEffectHitData(IEffectDealer dealer)
        {
            //FIXME: 要用pool, 泛用的pool
            var data = new GeneralEffectHitData();
            data.Override(dealer, this);
            return data;
        }

        public void ForceDirectEffectHit(GeneralEffectDealer dealer)
        {
            var hitData = GenerateEffectHitData(dealer);
            dealer.OnHitEnter(hitData);
            OnEffectHitEnter(hitData);
            //然後要馬上離開？
            dealer.OnHitExit(hitData);
            OnEffectHitExit(hitData);
        }

        //收到事件後，叫下面的action做事
        public IEffectType getEffectType => _effectType;

        //FIXME: rename to OnHitEnter
        public void OnEffectHitEnter(IEffectHitData data, DetectData detectData) //這裡是code定義
        {
            _detectData = detectData;
            OnEffectHitEnter(data);
        }

        public void OnEffectHitEnter(IEffectHitData data)
        {
            this.Log("OnHitEnter");
            _currentHitData = data as GeneralEffectHitData;
            _enterNode?.EventHandle(data as GeneralEffectHitData);
            _dealers.Add(data.Dealer as GeneralEffectDealer);
#if UNITY_EDITOR
            _lastHitData = data;
#endif
        }

        public bool HasDealerOverlap => _dealers.Count > 0;

        [PreviewInInspector]
        private HashSet<GeneralEffectDealer> _dealers = new();

        public void OnEffectHitExit(IEffectHitData data)
        {
            this.Log("OnHitExit");
            _dealers.Remove(data.Dealer as GeneralEffectDealer);
            _exitNode?.EventHandle(data as GeneralEffectHitData);
            _currentHitData = null;
        }

        // public float ReactValue => ValueSource?.FinalValue ?? 0;

        //EffectExit也要呢
        protected override string TypeTag => "Receiver";

        public DetectData? GetDetectData()
        {
            if (_detectData.HasValue)
                return _detectData.Value;
            else
                return null; //或許可以拋出異常？
        }

        protected override string DescriptionTag => "Receiver";
    }
}

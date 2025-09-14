using System;
using System.Collections.Generic;
using MonoFSM.Core;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.Detection;
using MonoFSM.Foundation;
using MonoFSM.Variable.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSM.Runtime.Interact.EffectHit
{
    public abstract class BaseEffectDetectTarget : AbstractDescriptionBehaviour //實作
    {
        protected override void Start()
        {
            base.Start();
            if (_detectable == null)
            {
                _detectable = GetComponentInParent<EffectDetectable>();
                if (_detectable == null)
                    Debug.LogError(
                        "BaseEffectDetectTarget requires an EffectDetectable component on the same GameObject.",
                        this
                    );
            }
        }

        [AutoParent]
        private EffectDetectable _detectable;

        public EffectDetectable Detectable => _detectable; //動態生成的沒有綁定到？
        // public GeneralEffectReceiver[] EffectReceivers => _detectable.EffectReceivers;
    }

    [DisallowMultipleComponent]
    //空間中的物件，可以被偵測到, 基本上會有collider或是collider2D
    //從Detector過來
    public class EffectDetectable
        : MonoDict<GeneralEffectType, GeneralEffectReceiver>,
            IDefaultSerializable //關係
    {
        //可能不只一個？
        // [Obsolete("只是拿來新增用的button？其實不一定需要？")]
        [CompRef]
        [AutoChildren(DepthOneOnly = true)]
        [SerializeField]
        private BaseEffectDetectTarget[] _effectDetectTargets; //FIXME:不該？

        // [AutoParent] private StateMachineOwner owner;
        //
        // public StateMachineOwner Owner => owner;


        //FIXME: 確保layer有設定
        // [Component] [AutoChildren(DepthOneOnly = true)]
        // private GeneralEffectReceiver[] _effectReceivers;

        // [ShowInInspector] public GeneralEffectReceiver[] EffectReceivers => _effectReceivers;

        public GameObject TargetObject => gameObject;
        public bool IsValidTarget => enabled && gameObject.activeInHierarchy;

        //DebugOnly
#if UNITY_EDITOR
        [PreviewInInspector]
        public HashSet<EffectDetector> _debugDetectors = new(); //沒在判？
#endif

        // List<SpatialDetector> fromDetectors;
        [PreviewInInspector]
        private HashSet<EffectDetector> toRemoves = new();

        public void ProcessEffectHit(EffectDetector detector)
        {
            //FIXME: 在這邊new data...?
            var detectData = new DetectData(detector, this);

            foreach (var dealer in detector.Dealers)
            {
                var receiver = Get(dealer._effectType);
                if (receiver == null)
                    continue;
                // foreach (var receiver in EffectReceivers)
                // {
                //
                // }
                if (!dealer.CanHitReceiver(receiver))
                    continue;

                var hitData = receiver.GenerateEffectHitData(dealer);
                dealer.OnHitEnter(hitData, detectData);
                receiver.OnEffectHitEnter(hitData, detectData);
            }
        }

        //         private void OnDisable() //FIXME: 這是TriggerDetectableTarget該做的事嗎？
        //         {
        //             //FIXME: 標記狀態改變，不要在這裡執行OnSpatialExit?
        //             if (!Application.isPlaying)
        //                 return;
        // #if UNITY_EDITOR
        //             toRemoves.AddRange(_detectors);
        // #endif
        //             foreach (var toRemove in toRemoves)
        //             {
        //                 // Debug.Log("OnDisable of Detectable", this);
        //                 // Debug.Log("OnDisable of Detectable removef from" + toRemove, toRemove);
        //                 toRemove.OnDetectExitCheck(gameObject);
        //
        //                 //copy _detectedObjects to toRemove
        //                 // toRemove.AddRange(_detectedObjects);
        //                 // foreach (var detectable in toRemove)
        //                 // {
        //                 //     // Debug.Log("OnDisable of detectable",detectable);
        //                 //     OnTriggerExit(detectable.MyCollider);
        //                 // }
        //                 // toRemove.Clear();
        //             }
        //
        //             toRemoves.Clear();
        //         }

        // protected override string DescriptionTag => "Detectable 接收";
        protected override void AddImplement(GeneralEffectReceiver item) { }

        protected override void RemoveImplement(GeneralEffectReceiver item) { }

        protected override bool CanBeAdded(GeneralEffectReceiver item)
        {
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MonoFSM.Core.Detection;
using MonoFSM.Core.Runtime.Interact.SpatialDetection;
using MonoFSM.Runtime.Interact.EffectHit;
using UnityEngine;

namespace MonoFSM.Core
{
    //FIXME: 看不懂，刪掉？ 往parent找是什麼意思？
    // public class ParentTargetDetector : BaseDetectProcessor
    // {
    //     private EffectDetectable _parentDetectable;
    //
    //     private void OnEnable()
    //     {
    //         OnEnableNextFrame().Forget();
    //     }
    //
    //     private async UniTaskVoid OnEnableNextFrame()
    //     {
    //         //FIXME: 應該要等多久？等到LateUpdate？還是等到下一幀？
    //         await UniTask.WaitForEndOfFrame();
    //         _parentDetectable = GetComponentInParent<EffectDetectable>();
    //         if (_parentDetectable == null)
    //             Debug.LogError("ParentTargetDetector requires an EffectDetectable component in the parent hierarchy.",
    //                 this);
    //
    //         OnDetectEnterCheck(_parentDetectable.gameObject);
    //     }
    //
    //
    //     protected override void OnDisableImplement()
    //     {
    //         if (_parentDetectable != null)
    //             OnDetectExit(_parentDetectable.gameObject);
    //     }
    //
    //     protected override void SetLayerOverride()
    //     {
    //     }
    //
    //     public override IEnumerable<DetectionResult> GetCurrentDetections()
    //     {
    //         throw new NotImplementedException();
    //     }
    //
    //     public override void UpdateDetection()
    //     {
    //         throw new NotImplementedException();
    //     }
    // }
}

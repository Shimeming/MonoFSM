using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.Runtime.Action;
using UnityEngine;

//DElayNode?
//FIXME: 很危險，可能因為切state delay還沒結束結果沒有觸發
//Note: 還是直接切一個State出來比較好，用timer決定要去
//因為是view, 所以沒差
//哇要區分view和server logic可以用的api...hmmm
[Obsolete]
public class DelayActionModifier : MonoBehaviour
{
    public float delayTime = 1;

    [Component(AddComponentAt.Children, "[Action]")]
    [PreviewInInspector]
    [AutoChildren]
    private AbstractStateAction[] actions;

    //TODO: state change 可以？
    public async UniTaskVoid TriggerDelayAction()
    {
        //FIXME: 用update不好...hmm
        await UniTask.Delay(
            TimeSpan.FromSeconds(delayTime),
            DelayType.DeltaTime,
            PlayerLoopTiming.Update,
            CancellationToken.None //怎麼弄...
        );
    }
    // private void AddAction()
    // {
    // }
}

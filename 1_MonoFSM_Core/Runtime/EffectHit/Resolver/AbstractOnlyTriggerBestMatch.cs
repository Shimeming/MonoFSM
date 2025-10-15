using MonoFSM.Core.Detection;
using MonoFSM.Foundation;
using UnityEngine;

namespace MonoFSM.Runtime.Interact.EffectHit
{
    /// <summary>
    /// 抽象類：標記 EffectDealer 只觸發最符合的 EffectReceiver
    /// 繼承此類並實現自定義的評分邏輯
    /// </summary>
    public abstract class AbstractOnlyTriggerBestMatch : MonoBehaviour
    {
        /// <summary>
        /// 計算 Receiver 的匹配分數
        /// 分數越高表示越適合
        /// </summary>
        /// <param name="dealer">發起判定的 Dealer</param>
        /// <param name="receiver">候選的 Receiver</param>
        /// <param name="detectData">檢測數據</param>
        /// <returns>匹配分數，分數越高越優先</returns>
        public abstract float CalculateScore(
            GeneralEffectDealer dealer,
            GeneralEffectReceiver receiver
        // DetectData detectData
        );

        // protected override string DescriptionTag => "BestMatch";
    }
}

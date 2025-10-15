using MonoFSM.Core.Detection;
using UnityEngine;

namespace MonoFSM.Runtime.Interact.EffectHit
{
    /// <summary>
    /// 預設的 Best Match 評分器
    /// 基於優先級和距離計算分數
    /// </summary>
    public class DefaultBestMatchScorer : AbstractOnlyTriggerBestMatch
    {
        [Header("評分權重")]
        [Tooltip("優先級的權重係數")]
        [SerializeField]
        private float _priorityWeight = 1000f;

        [Tooltip("是否考慮距離（距離越近分數越高）")]
        [SerializeField]
        private bool _useDistanceScore = true;

        public override float CalculateScore(
            GeneralEffectDealer dealer,
            GeneralEffectReceiver receiver
        )
        {
            float score = 0f;

            // 優先級分數（最重要）
            score += receiver.MatchPriority * _priorityWeight;

            // 距離分數（越近越好）
            if (_useDistanceScore)
            {
                var distance = Vector3.Distance(
                    dealer.transform.position,
                    receiver.transform.position
                );
                score -= distance; // 距離越小，分數越高
            }

            return score;
        }
    }
}

using MonoFSM.Core.DataProvider;
using MonoFSM.Core.Runtime.Action;
using MonoFSM.Runtime.Interact.EffectHit;
using MonoFSM.Variable;
using MonoFSM.Variable.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace MonoFSM_Physics.Runtime.PhysicsAction
{
    /// <summary>
    /// 對一個 Rigidbody 施加力的 Action
    /// 支援通過預設 Rigidbody、Provider 或者 GeneralEffectHitData 來取得目標
    /// </summary>
    public class AddForceAction : AbstractStateAction, IArgEventReceiver<GeneralEffectHitData>
    {
        [Header("Rigidbody Target")]
        [Tooltip("直接指定的 Rigidbody，優先級最高")]
        // [DropDownRef] private Rigidbody _rigidbody;
        //
        // [HideIf("_rigidbody")]
        // [Tooltip("當沒有直接指定 Rigidbody 時使用的 Provider")]
        // [SerializeField] private ValueProvider _rigidbodyProvider;
        public VarComp _rigidbodyVar;

        [Header("Force Settings")]
        // [CompRef]
        // [AutoChildren]
        [Tooltip("力的方向提供器")]
        // private IValueProvider<Vector3> _forceDirectionProvider;
        public VarVector3 _forceDirectionVar;

        [Tooltip("力的大小")]
        [FormerlySerializedAs("_torqueMagnitude")]
        [SerializeField]
        private float _magnitude = 10f;

        [Tooltip("力的施加模式")]
        [SerializeField]
        private ForceMode _forceMode = ForceMode.Impulse;

        [Tooltip("力的施加位置")]
        public ForcePosition _forcePosition = ForcePosition.TargetCenterOfMass;

        public enum ForcePosition
        {
            TargetCenterOfMass, // 使用剛體的質心
            ActionPosition // 使用 Action 所在的 Transform 位置
            ,
        }

        /// <summary>
        /// 對指定的 Rigidbody 施加力
        /// </summary>
        /// <param name="target">目標 Rigidbody</param>
        /// <param name="force">施加的力</param>
        public void AddForceToRigidbody(Rigidbody target, Vector3 force)
        {
            if (target == null)
            {
                Debug.LogError("No Rigidbody provided to AddForceAction", this);
                return;
            }

            // 優先使用自定義的 Rigidbody 實作
            if (target.TryGetComponent<ICustomRigidbody>(out var customRigidbody))
            {
                customRigidbody.AddForce(force, _forceMode);
                return;
            }

            Debug.Log($"AddForce: Applying force to {target.name} with direction: {force}", this);
            DrawArrow.ForDebug(target.position, force.normalized, Color.green);
            // 確定力的施加位置
            Vector3 forcePosition =
                _forcePosition == ForcePosition.ActionPosition
                    ? transform.position
                    : target.worldCenterOfMass;

            // 使用 AddForceAtPosition 來施加力
            target.AddForceAtPosition(force, forcePosition, _forceMode);
        }

        public new void EventReceived()
        {
            OnActionExecuteImplement();
        }

        public new bool IsValid => this != null && gameObject.activeInHierarchy;
        public new bool isActiveAndEnabled => enabled && gameObject.activeInHierarchy;

        /// <summary>
        /// 執行 Action 的實作，當沒有外部事件觸發時使用
        /// </summary>
        protected override void OnActionExecuteImplement()
        {
            ExecuteForceAction();
        }

        /// <summary>
        /// 執行力施加的核心邏輯
        /// </summary>
        /// <param name="hitData">撞擊資料，可為 null</param>
        /// <param name="fallbackRigidbodySource">備用的 Rigidbody 來源</param>
        private void ExecuteForceAction(
            GeneralEffectHitData hitData = null,
            Transform fallbackRigidbodySource = null
        )
        {
            Rigidbody targetRigidbody = GetTargetRigidbody(fallbackRigidbodySource);
            if (targetRigidbody == null)
            {
                Debug.LogError("No valid Rigidbody found for AddForceAction", this);
                return;
            }

            Vector3 force = CalculateForceVector(hitData);
            AddForceToRigidbody(targetRigidbody, force);
        }

        /// <summary>
        /// 取得目標 Rigidbody 的統一方法
        /// </summary>
        /// <param name="fallbackSource">當預設來源都無效時的備用來源</param>
        /// <returns>目標 Rigidbody，如果找不到則返回 null</returns>
        private Rigidbody GetTargetRigidbody(Transform fallbackSource = null)
        {
            // 優先使用直接指定的 rigidbody

            return _rigidbodyVar.Value as Rigidbody;
            // 最後嘗試從備用來源取得 rigidbody
            if (fallbackSource != null)
            {
                return fallbackSource.GetComponent<Rigidbody>();
            }

            return null;
        }

        /// <summary>
        /// 計算力方向的統一方法
        /// </summary>
        /// <param name="hitData">撞擊資料，可為 null</param>
        /// <returns>計算後的力向量</returns>
        private Vector3 CalculateForceVector(GeneralEffectHitData hitData = null)
        {
            // 如果有撞擊資料且包含法向量，優先使用
            // if (hitData?.hitNormal.HasValue == true)
            // {
            //     return hitData.hitNormal.Value.normalized * _magnitude;
            // }
            var start = hitData.GeneralDealer.transform.position;
            var hitPoint = hitData.hitPoint.Value;
            var dir = (hitPoint - start).normalized;
            return dir * _magnitude;

            return _forceDirectionVar.GetValue() * _magnitude;
            // 否則使用預設的力方向
            // return _forceDirectionProvider.Get<Vector3>() * _magnitude;
        }

        public void ArgEventReceived(GeneralEffectHitData arg)
        {
            Transform fallbackSource = arg?.Receiver?.transform;
            ExecuteForceAction(arg, fallbackSource);
        }
    }
}

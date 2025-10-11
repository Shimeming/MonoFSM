using MonoFSM.Core.Attributes;
using MonoFSM.Core.Simulate;
using MonoFSM.Foundation;
using MonoFSMCore.Runtime.LifeCycle;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSM.Core.Condition
{
    //這個要整個Panel OnEnable的時候才會檢查一遍，不會隨時檢查
    //ActivateChecker
    public abstract class //IReturnToPool? IDespawn?
    AbstractConditionActivateTarget : AbstractDescriptionBehaviour, IUpdateSimulate, IResetStart //, ISelectedInstanceUpdater //ISubmitHandler
    {
        protected override string DescriptionTag => "Condition Activate";

        /// <summary>
        /// 要不要做成disable還會檢查的simulate?
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Simulate(float deltaTime)
        {
            ActivateCheck();
        }

        public void AfterUpdate() { }

        [InlineField]
        [AutoNested]
        public ConditionGroup _conditionGroup;

        // [PreviewInInspector] protected virtual bool result => _conditionGroup.IsValid;

        public void ActivateCheck()
        {
            ActivateCheckImplement(_conditionGroup.IsValid);
        }

        protected abstract void ActivateCheckImplement(bool isValid); //last result?

        public void ResetStart() //開始時先檢查
        {
            ActivateCheck();
        }
    }
}

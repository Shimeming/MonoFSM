using MonoFSM.Core.Simulate;
using MonoFSMCore.Runtime.LifeCycle;
using UnityEngine;

namespace MonoFSM.Core.Condition
{
    //這個要整個Panel OnEnable的時候才會檢查一遍，不會隨時檢查
    //ActivateChecker
    public abstract class //IReturnToPool? IDespawn?
    AbstractConditionActivateTarget : MonoBehaviour, IUpdateSimulate, IResetStart //, ISelectedInstanceUpdater //ISubmitHandler
    {
        public void Simulate(float deltaTime)
        {
            ActivateCheck();
        }

        public void AfterUpdate() { }

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

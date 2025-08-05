using MonoFSM.Core.DataProvider;
using MonoFSM.Core.Runtime.Action;
using MonoFSM.Runtime;
using MonoFSM.Runtime.Attributes;
using UnityEngine;

namespace _1_MonoFSM_Core.Runtime.Action.TransformAction
{
    //TODO: abstract化兩個Entity之間的作用
    public class SetTransformParentAction : AbstractStateAction
    {
        // public Transform _target;

        [ValueTypeValidate(typeof(MonoEntity))] [DropDownRef]
        public ValueProvider _sourceValueProvider;

        // public VarCompProviderRef _targetVarRef; //拿到rigidbody的話，再拿transform
        [ValueTypeValidate(typeof(MonoEntity))] //FIXME: validation沒有辦法提示嗎
        [DropDownRef]
        public ValueProvider _targetValueProvider; 

        protected override void OnActionExecuteImplement()
        {
            var targetEntity = _targetValueProvider.Get<MonoEntity>();
            // var comp = _targetVarRef.Value;
            // if (comp == null)
            // {
            //     Debug.LogError("Target component is null. Cannot set parent.", this);
            //     return;
            // }
            var sourceTransform = _sourceValueProvider.Get<MonoEntity>().transform;
            var targetTransform = targetEntity?.transform;
            // _sourceValueProvider.Get<MonoEntity>().transform.SetParent(targetEntity.transform);
            if (targetTransform == null)
            {
                Debug.LogError("Target transform is null. Cannot set parent.", this);
                return;
            }

            if (sourceTransform == null)
            {
                Debug.LogError("Source transform is null. Cannot set parent.", this);
                return;
            }

            sourceTransform.SetParent(targetTransform);
        }
    }
}
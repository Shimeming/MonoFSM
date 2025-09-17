using UnityEngine;

namespace MonoFSM.Variable.Condition
{
    public class IsUnityObjectVariableNullCondition : AbstractConditionBehaviour
    {
        [DropDownRef]
        public AbstractMonoVariable unityObjectVariable;

        //FIXME: Variable Tag？
        protected override bool IsValid => unityObjectVariable.Get<Object>() != null;
    }
}

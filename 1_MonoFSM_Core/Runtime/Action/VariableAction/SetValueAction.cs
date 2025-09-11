using MonoFSM.Core.Runtime.Action;
using MonoFSM.Variable;
using UnityEngine;

namespace _1_MonoFSM_Core.Runtime.Action.VariableAction
{
    //SetObjectValue?
    public class SetValueAction : AbstractStateAction
    {
        [SerializeField]
        private AbstractMonoVariable _sourceVar;

        [SerializeField]
        private AbstractMonoVariable _targetVar;

        //FIXME: validate: 兩個要同型別? 還是value type要可以assign上去就好？
        protected override void OnActionExecuteImplement()
        {
            //用objectValue有點討厭, 還是乾脆每種都寫？ObjectValue會有裝箱問題
            _sourceVar.SetValue(_targetVar.objectValue, this);
        }
    }
}

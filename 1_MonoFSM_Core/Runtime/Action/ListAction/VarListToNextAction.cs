using MonoFSM.Core.Runtime.Action;
using MonoFSM.Core.Variable;
using UnityEngine;

namespace _1_MonoFSM_Core.Runtime.Action.ListAction
{
    public class VarListToNextAction : AbstractStateAction
    {
        [SerializeField]
        [DropDownRef]
        private AbstractVarList _varList; //valueProvider?

        protected override void OnActionExecuteImplement()
        {
            _varList.GoToNext();
        }
    }
}

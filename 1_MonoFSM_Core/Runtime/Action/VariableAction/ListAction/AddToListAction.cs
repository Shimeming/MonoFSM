using MonoFSM.Core.DataProvider;
using MonoFSM.Core.Runtime.Action;
using MonoFSM.Core.Variable;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _1_MonoFSM_Core.Runtime.Action.VariableAction.ListAction
{
    //ListManipulationAction?
    public class AddToListAction : AbstractStateAction
    {
        //用source & target?

        // [Required] [CompRef] [AutoChildren] private SourceValueRef _sourceValueRef; //這個也dropdownRef?
        [DropDownRef] [SerializeField] private ValueProvider _itemValueProvider;

        [DropDownRef]
        [Required] [SerializeField] private AbstractVarList _targetList; //FIXME: 這個型別可以抽象掉嗎？

        protected override void OnActionExecuteImplement()
        {
            //型別爆掉自己負責？object一定可以...?
            //FIXME 先用object吧？
            //FIXME: 應該可以用generic? 和 SetValueByValueProvider 一樣嗎
            _targetList.Add(_itemValueProvider.Get<Object>());
        }
    }
}
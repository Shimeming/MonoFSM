using MonoFSM.Foundation;
using MonoFSM.Runtime;
using UnityEngine;

namespace MonoFSM.Core.Variable.Providers
{
    public class CurrentItemOfListProvider : AbstractCurrentItemOfListProvider<MonoEntity> { }

    public class AbstractCurrentItemOfListProvider<T> : AbstractValueProvider<T>
    {
        public override string Description => "Get Current Item " + _varList?.name;

        [DropDownRef]
        [SerializeField]
        private VarList<T> _varList;
        public override T Value => _varList != null ? _varList.CurrentListItem : default;
    }
}

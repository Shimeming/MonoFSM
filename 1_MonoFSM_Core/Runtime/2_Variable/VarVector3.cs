using MonoFSM.EditorExtension;
using UnityEngine;

namespace MonoFSM.Variable
{
    public class VarVector3
        : AbstractFieldVariable<GameDataVector3, FlagFieldVector3, Vector3>, //可以改成?嗎？
            IHierarchyValueInfo
    {
        public string ValueInfo => CurrentValue.ToString();
        public bool IsDrawingValueInfo => true;

        public override bool IsValueExist => !IsNull;

        //FIXME: 另外寫nullable? 用一個bool過？hmmm 到了要清掉這樣嗎？
    }
}

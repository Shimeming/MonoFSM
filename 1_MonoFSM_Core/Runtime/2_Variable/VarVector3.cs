using MonoFSM.EditorExtension;
using UnityEngine;

namespace MonoFSM.Variable
{
    public class VarVector3
        : AbstractFieldVariable<GameDataVector3, FlagFieldVector3, Vector3>,
            IHierarchyValueInfo
    {
        public string ValueInfo => CurrentValue.ToString();
        public bool IsDrawingValueInfo => true;
        public override bool IsValueExist => CurrentValue != Vector3.zero;
    }
}

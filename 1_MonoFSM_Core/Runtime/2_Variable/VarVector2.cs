using MonoFSM.EditorExtension;
using UnityEngine;

namespace MonoFSM.Variable
{
    public class VarVector2
        : AbstractFieldVariable<GameDataVector2, FlagFieldVector2, Vector2>,
            IHierarchyValueInfo
    {
        public string ValueInfo => CurrentValue.ToString();
        public bool IsDrawingValueInfo => true;
        public override bool IsValueExist => CurrentValue != Vector2.zero;
    }
}

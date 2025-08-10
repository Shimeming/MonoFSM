using RCGExtension;
using UnityEngine;
using MonoFSM.Core.Attributes;
using MonoFSM.Variable.FieldReference;

namespace MonoFSM.Variable
{
    public class VarVector2 :
    GenericMonoVariable<GameDataVector2, FlagFieldVector2, Vector2?>,
        IHierarchyValueInfo
    {
        public string ValueInfo => CurrentValue.ToString();
        public bool IsDrawingValueInfo => true;
        public override bool IsValueExist => CurrentValue.HasValue;
    }
}
using MonoFSM.Variable;
using MonoFSM.Variable.FieldReference;
using MonoFSMCore.Runtime.LifeCycle;

namespace MonoFSM.Core.Variable
{
    [FormerlyNamedAs("VarPoolObj")]
    public class VarMonoObj : GenericUnityObjectVariable<MonoObj>
    {
        //FIxME: 要區分Prefab和Runtime Object嗎？ 提示？
    }
}
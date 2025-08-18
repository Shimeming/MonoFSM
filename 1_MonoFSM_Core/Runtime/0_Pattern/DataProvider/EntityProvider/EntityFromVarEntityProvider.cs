using MonoFSM.Runtime;
using MonoFSM.Runtime.Variable;
using Sirenix.OdinInspector;

namespace MonoFSM.Core.Runtime
{
    //這個還不錯，可以從一個VarEntity拿到MonoEntity就作為ValueProvider的起點
    public class EntityFromVarEntityProvider : AbstractEntityProvider
    {
        public override string SuggestDeclarationName => _varEntity._varTag?.name;
        public override MonoEntity monoEntity => _varEntity?.Value;

        [PropertyOrder(-1)]
        [DropDownRef] public VarEntity _varEntity;
        //ValueProvider放Children?
    }
}

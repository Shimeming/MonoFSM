using MonoFSM.Runtime;
using MonoFSM.Runtime.Variable;

namespace MonoFSM.Core.Runtime
{
    public class EntityFromVarEntityProvider : AbstractEntityProvider
    {
        public override string SuggestDeclarationName => _varEntity._varTag?.name;
        public override MonoEntity monoEntity => _varEntity?.Value;
        [DropDownRef] public VarEntity _varEntity;
    }
}

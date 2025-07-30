using MonoFSM.Runtime;
using Sirenix.OdinInspector;

namespace MonoFSM.Core.Runtime
{
    //Self?
    //改名，這個記不住，OwnerEntity? ParentEntity? 
    //還是要EntityRef? 直接啦？
    public class ParentEntityProvider : AbstractEntityProvider, IEntityProvider
    {
        public override string SuggestDeclarationName => "this";

        //為什麼不會自己撈？
        [ShowInInspector] [Required] [AutoParent]
        private MonoEntity _parentEntity;

        // [PreviewInInspector] public MonoEntity monoEntity => _monoBlackboard;
        // public MonoEntityTag entityTag => _monoBlackboard?.Tag;
        public override MonoEntity monoEntity => _parentEntity;
    }
    //Description應該要套娃？
}
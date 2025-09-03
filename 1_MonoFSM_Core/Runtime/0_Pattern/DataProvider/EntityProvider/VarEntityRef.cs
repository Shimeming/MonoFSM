using MonoFSM.Foundation;
using MonoFSM.Runtime;
using MonoFSM.Runtime.Mono;
using MonoFSM.Runtime.Variable;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSM.Core.Runtime
{
    //FIXME: 好像可以留著喔？重寫？
    //provider是從variable拿到的MonoEntity，這個MonoEntity是
    // [Obsolete]
    public class VarEntityRef : AbstractValueProvider<MonoEntity>
    {
        public override string Description => $"Var Entity: {_varEntity?.name ?? "None"}";

        // [ValueTypeValidate(typeof(MonoEntity))] [SerializeField]
        // private ValueProvider _varEntityProvider;
        //FIXME: 必定從自身拿到？
        [DropDownRef]
        [SerializeField]
        private VarEntity _varEntity;
        public MonoEntity monoEntity => _varEntity?.Value;

        [ShowInInspector]
        public MonoEntityTag entityTag => monoEntity?.DefaultTag ?? _varEntity?._monoEntityTag; //FIXME: monoEntity是null, 這樣 _varEntity要先知道有什麼tag?

        public override MonoEntity Value => monoEntity;
    }
}

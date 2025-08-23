using MonoFSM.Core.DataProvider;
using MonoFSM.Runtime;
using MonoFSM.Runtime.Attributes;
using UnityEngine;

namespace MonoFSM.Core.Runtime
{
    // [Obsolete]
    //EntityRefFromValueProvider
    public class EntityFromValueProvider : AbstractEntityProvider
    {
        public override string SuggestDeclarationName => "entity:";
        public override MonoEntity monoEntity => _monoEntityProvider?.Get<MonoEntity>();

        [ValueTypeValidate(typeof(MonoEntity))]
        [SerializeField]
        [DropDownRef]
        private ValueProvider _monoEntityProvider;
    }
}

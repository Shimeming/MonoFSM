using MonoFSM.Core.Attributes;
using MonoFSM.Runtime;
using MonoFSM.Runtime.Variable;
using MonoFSM.Variable;

namespace _1_MonoFSM_Core.Runtime.EffectHit
{
    public abstract class AbstractEntityInteractCondition : AbstractConditionBehaviour
    {
        //runtime assign?
        [PreviewInInspector]
        public MonoEntity _sourceEntity; //用{get;set;}?

        [PreviewInInspector]
        public MonoEntity _targetEntity;
        protected abstract override bool IsValid { get; } //透過sourceEntity和_給一段邏輯？
    }
}

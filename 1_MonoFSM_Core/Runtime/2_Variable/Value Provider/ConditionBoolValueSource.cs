using MonoFSM.Foundation;
using MonoFSM.Variable.Attributes;
using Sirenix.OdinInspector;

namespace Fusion.Addons.KCC.ECM2.Examples.Networking.Fusion_v2.Characters.Scripts.Input
{
    public class ConditionBoolValueSource : AbstractValueSource<bool>
    {
        [Required]
        [CompRef]
        [Auto]
        AbstractConditionBehaviour _condition;
        public override bool Value => _condition?.FinalResult ?? false;
    }
}

using MonoFSM.Foundation;

namespace Fusion.Addons.KCC.ECM2.Examples.Networking.Fusion_v2.Characters.Scripts.Input
{
    public class IntToFloatValueSource : AbstractValueSource<float>
    {
        public VarInt _intVar;
        public override float Value => _intVar != null ? _intVar.Value : 0f;
    }
}

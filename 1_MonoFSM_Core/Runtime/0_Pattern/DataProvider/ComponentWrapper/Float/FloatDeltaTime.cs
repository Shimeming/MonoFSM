using MonoFSM.Core.DataProvider;
using MonoFSM.Core.Simulate;
using MonoFSM.Foundation;
using MonoFSM.Runtime;

namespace _1_MonoFSM_Core.Runtime._0_Pattern.DataProvider.ComponentWrapper.Float
{
    public class FloatDeltaTime : AbstractDescriptionBehaviour, IFloatProvider
    {
        protected override string DescriptionTag => "Float";
        public float Value => WorldUpdateSimulator.DeltaTime;
    }
}

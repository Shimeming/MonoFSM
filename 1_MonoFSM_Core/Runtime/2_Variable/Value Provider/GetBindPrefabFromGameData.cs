using MonoFSM.Core.DataProvider;
using MonoFSM.Foundation;
using MonoFSM.Variable;
using MonoFSMCore.Runtime.LifeCycle;

namespace Fusion.Addons.KCC.ECM2.Examples.Networking.Fusion_v2.Characters.Scripts.Input
{
    //反向取值..
    public class GetBindPrefabFromGameData : AbstractGetter, IValueProvider<MonoObj>
    {
        public VarGameData _gameData;

        public override bool HasValue =>
            _gameData != null && _gameData.Value != null && _gameData.Value.bindPrefab != null;

        public MonoObj Value => HasValue ? _gameData.Value.bindPrefab : null;
    }
}

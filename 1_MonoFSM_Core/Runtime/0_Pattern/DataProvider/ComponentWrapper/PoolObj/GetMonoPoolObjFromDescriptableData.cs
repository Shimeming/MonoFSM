using MonoFSM.Core.Attributes;
using MonoFSM.Variable.Attributes;
using MonoFSMCore.Runtime.LifeCycle;
using UnityEngine;

namespace MonoFSM.Core.DataProvider.ComponentWrapper
{
    public class GetMonoPoolObjFromDescriptableData : MonoBehaviour, ICompProvider<MonoObj>
    {
        [CompRef] [Auto] private IGameDataProvider _gameDataProvider;

        public string Description => "Get MonoPoolObj from DescriptableData";

        [ShowInPlayMode]
        public MonoObj Get()
        {
            return _gameDataProvider.GameData.bindPrefab;
        }

        Component ICompProvider.Get()
        {
            return Get();
        }
    }
}
using MonoFSMCore.Runtime.LifeCycle;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace MonoFSM.Core.DataProvider.ComponentWrapper
{
    //DirectReference
    public class MonoPoolObjRef : MonoBehaviour, IMonoObjectProvider
    {
        [FormerlySerializedAs("_monoPoolObj")] [Required]
        public MonoObj _monoObj;

        public string Description => $"MonoPoolObj Reference: {_monoObj.name}";

        public MonoObj Get()
        {
            return _monoObj;
        }
    }
}
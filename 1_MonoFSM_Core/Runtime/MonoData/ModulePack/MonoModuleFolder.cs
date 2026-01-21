using _1_MonoFSM_Core.Runtime.Attributes;
using UnityEngine;

namespace _1_MonoFSM_Core.Runtime.LifeCycle.Update
{
    public class MonoModuleFolder : MonoBehaviour
    {
        [PrefabFilter(typeof(MonoModulePack))] public MonoModulePack[] PrefabModules;
    }
}

using _1_MonoFSM_Core.Runtime.EffectHit.Resolver;
using _1_MonoFSM_Core.Runtime.FSMCore.Core.StateBehaviour;
using MonoFSM.Core.Attributes;
using UnityEngine;

namespace _1_MonoFSM_Core.Runtime.LifeCycle.Update
{
    public class MonoModulePack : MonoBehaviour
    {
        [PreviewInInspector] [AutoChildren] VariableFolder _variableFolder;
        [PreviewInInspector] [AutoChildren] MonoFSMOwner _monoFsmOwner;
        [PreviewInInspector] [AutoChildren] EffectsFolder _effectsFolder;
    }
}

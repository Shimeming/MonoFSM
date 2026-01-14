using _1_MonoFSM_Core.Runtime.FSMCore.Core.StateBehaviour;
using MonoFSM.Core;
using MonoFSM.CustomAttributes;
using MonoFSM.Runtime.Interact.EffectHit;
using MonoFSM.Variable;
using MonoFSM.Variable.Attributes;
using UnityEngine;

namespace _1_MonoFSM_Core.Runtime.LifeCycle.Update
{
    public class MonoModulePack : MonoBehaviour, IDropdownRoot
    {
        [CompRef] [AutoChildren] public VariableFolder _variableFolder;
        [CompRef] [AutoChildren] public StateFolder _stateFolder;
        [CompRef] [AutoChildren] public EffectDetectable _detectable;
        [CompRef] [AutoChildren] public SchemaFolder _folder;

        /// <summary>
        /// 返回此 ModulePack 下所有的 MonoDictFolder
        /// </summary>
        public IMonoDictFolder[] GetAllFolders()
        {
            return GetComponentsInChildren<IMonoDictFolder>(true);
        }

        [CompRef] [AutoChildren] IMonoDictFolder[] _moduleFolders;
    }
}

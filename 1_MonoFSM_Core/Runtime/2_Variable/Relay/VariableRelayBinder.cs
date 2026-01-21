using System.Linq;

using UnityEngine;

using MonoFSM.Core.Attributes;
using MonoFSM.Foundation;

namespace MonoFSM.Variable
{
    //應該弄成folder比較好？
    public class VariableRelayBinder : AbstractDescriptionBehaviour
    {
        [Component] [PreviewInInspector] [AutoChildren]
        private VarBoolRelay[] _variableRelays;

        // [PreviewInInspector] [AutoChildren]
        // private VariableFolder[] _variableFolders; //從parent entity拿?
        // public VariableFolder VariableFolder => _variableFolders.FirstOrDefault();

        // public AbstractMonoVariable GetVar(VariableTag varTag)
        // {
        //     foreach (var folder in _variableFolders)
        //         if (folder.ContainsKey(varTag))
        //             return folder.Get(varTag);
        //
        //     Debug.LogError($"Variable {varTag} not found in any folder", this);
        //     return null;
        // }

        protected override string DescriptionTag => "RelayBinder";
    }
}

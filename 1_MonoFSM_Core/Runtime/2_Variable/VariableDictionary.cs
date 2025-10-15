using System;
using MonoFSM.Core;
using UnityEngine.Serialization;

namespace MonoFSM.Variable
{
    public class VariableDictionary : MonoDict<VariableTag, VarFloat>
    {
        protected override void AddImplement(VarFloat item) { }

        protected override void RemoveImplement(VarFloat item) { }

        protected override bool CanBeAdded(VarFloat item) => item.isActiveAndEnabled;

        protected override string DescriptionTag => "VarDict";
    }

    [Serializable]
    public class VirtualFloat : IFloatValueProvider
    {
        [AutoParent]
        VariableDictionary injectedVariables; //其實這個用 autoparent應該要可以

        //先找一個singleton monobehaviour, 然後
        [FormerlySerializedAs("VariableTypeTag")]
        [FormerlySerializedAs("variableTag")]
        public VariableTag VariableTag;

        public float FinalValue => injectedVariables[VariableTag].FinalValue;
    }
}

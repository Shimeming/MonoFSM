using System;

using UnityEngine;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.DataProvider;

namespace MonoFSM.Variable
{
    public interface ISerializedFloatValue
    {
        float EditorValue { get; set; }
    }

    [Obsolete]
    public interface IFloatValueProvider
    {
        [PreviewInInspector] float FinalValue { get; }

        //要可以set?
    }

    public interface IBoolProvider
    {
        bool IsTrue { get; }
    }

    // [InlineProperty]
    // [Serializable]
    // public class BoolValueSource : InterfaceMonoRef<StateMachineOwner, IBoolValue>, IBoolValue
    // {
    //     //從StateMachineOwner下面找到所有的IBoolValue
    //     public bool IsTrue => ((IBoolValue)ValueSource).IsTrue; //FIXME: 如果ValueSource是null, 不好debug...
    // }
}

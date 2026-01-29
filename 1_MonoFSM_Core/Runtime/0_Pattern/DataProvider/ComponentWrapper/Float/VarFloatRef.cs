using System;
using MonoFSM.DataProvider;
using MonoFSM.Core.DataProvider;
using MonoFSM.Foundation;
using MonoFSM.Variable;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSM.Core.Runtime._0_Pattern.DataProvider.ComponentWrapper
{
    public class VarFloatRef : AbstractValueSource<float>, IFloatProvider
    {
        //不對啊XDD
        [Required] [DropDownRef] public VarFloat _dropDownRef;

        public float GetFloat()
        {
            return _dropDownRef.Value;
        }

        // public float Value => _dropDownRef.Value;
        public override float Value => _dropDownRef != null ? _dropDownRef.Value : 0f;
        public Type ValueType => typeof(float);

        public override string Description => "DropDownRef: " + _dropDownRef?.Description;
    }
}

using System;
using MonoFSM.Foundation;
using UnityEngine;

namespace MonoFSM.Core.Variable.Providers
{
    public class GetCurrentItemOfVarList : AbstractGetter, IValueProvider
    {
        [SerializeField]
        private AbstractVarList _varList;
        public override bool HasValue => _varList != null && _varList.CurrentRawObject != null;

        public T1 Get<T1>()
        {
            if (_varList.CurrentRawObject is T1 t1)
                return t1;
            return default;
        }

        public Type ValueType => _varList.ValueType;
    }
}

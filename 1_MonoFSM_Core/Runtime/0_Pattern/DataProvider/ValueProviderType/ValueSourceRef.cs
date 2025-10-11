using System;
using MonoFSM.Foundation;
using MonoFSM.Variable.Attributes;
using UnityEngine;

namespace MonoFSM.Core.DataProvider.ValueProviderType
{
    //glue code...好像很多餘?
    [Obsolete]
    public class ValueSourceRef<T> : AbstractValueSource<T>
    {
        //FIXME: 沒有validate過程
        [CompRef]
        [SerializeField]
        private ValueProvider _valueProvider;

        public override T Value => _valueProvider != null ? _valueProvider.Get<T>() : default;
    }
}

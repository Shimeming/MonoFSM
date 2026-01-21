using MonoFSM.Core;
using MonoFSM.Foundation;
using MonoFSM.Variable.Attributes;
using UnityEngine;

namespace MonoFSM.Variable
{
    public abstract class TypedMonoVariable<T> : AbstractMonoVariable //, ISettable<T>
    {
        [CompRef]
        [AutoChildren(DepthOneOnly = true, _isSelfInclude = true)]
        protected IValueProvider[] _valueSources;

        protected IValueProvider valueSource => GetActiveTypedValueSource();

        protected IValueProvider GetActiveTypedValueSource()
        {
            //是這個沒有跑嗎？
            AutoAttributeManager.AutoReferenceFieldEditor(this, nameof(_valueSources));
            return ValueResolver.GetActiveValueSource(_valueSources, this);
        }

        protected override bool HasValueProvider
        {
            get
            {
                // Debug.Log("Check HasValueProvider in TypedMonoVariable");
                AutoAttributeManager.AutoReferenceFieldEditor(this, nameof(_valueSources));
                // Debug.Log("_valueSources.length" + _valueSources.Length);
                return ValueResolver.HasValueProvider(_valueSources) || base.HasValueProvider;
            }
        }

        public abstract void CommitValue();
    }
}

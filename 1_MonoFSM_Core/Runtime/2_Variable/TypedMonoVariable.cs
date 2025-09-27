using MonoFSM.Core;
using MonoFSM.Foundation;
using MonoFSM.Variable.Attributes;

namespace MonoFSM.Variable
{
    public abstract class TypedMonoVariable<T> : AbstractMonoVariable //, ISettable<T>
    {
        [CompRef]
        [AutoChildren(DepthOneOnly = true, _isSelfInclude = true)]
        //fixme; 用interface？好像可以改成AbstractValueProvider<T>? 但無型別的ValueProvider比較彈性
        protected IValueProvider[] _valueSources;

        protected IValueProvider valueSource => GetActiveTypedValueSource();

        protected IValueProvider GetActiveTypedValueSource()
        {
            AutoAttributeManager.AutoReferenceFieldEditor(this, nameof(_valueSources));
            return ValueResolver.GetActiveValueSource(_valueSources, this);
        }

        protected override bool HasValueProvider
        {
            get
            {
                AutoAttributeManager.AutoReferenceFieldEditor(this, nameof(_valueSources));
                return ValueResolver.HasValueProvider(_valueSources) || base.HasValueProvider;
            }
        }

        public abstract void CommitValue();
    }
}

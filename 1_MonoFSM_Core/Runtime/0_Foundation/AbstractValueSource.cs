using MonoFSM.Core;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.DataProvider;
using MonoFSM.Runtime;
using MonoFSM.Runtime.Mono;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSM.Foundation
{
    //寫的好美XD
    public abstract class AbstractGetter : AbstractDescriptionBehaviour //提供數值
    {
        protected override bool IsBracketsNeededForTag => false;
        protected override string DescriptionTag => "=>";

        [AutoNested]
        [InlineField]
        [PropertyOrder(1)]
        public ConditionGroup _conditionGroup;

        public virtual bool IsValid => _conditionGroup.IsValid && isActiveAndEnabled;
        public abstract bool HasValue { get; }
    }

    //FIXME: 感覺還是很容易會繼承錯...
    public abstract class AbstractEntitySource
        : AbstractValueSource<MonoEntity>,
            IEntityValueProvider //只有我需要特別寫對吧？
    {
        //FIXME: 需要提供EntityTag!
        public abstract MonoEntityTag entityTag { get; }
    }

    public abstract class AbstractValueSource<T> : AbstractGetter, IValueProvider<T> //提供數值
    {
        protected override string DescriptionTag => "=> (" + typeof(T).Name + ")";

        [AutoParent]
        private MonoEntity _monoEntity;

        // public MonoEntity ParentEntity =>
        //     _monoEntity ? _monoEntity : _monoEntity = GetComponentInParent<MonoEntity>(true);

        [ShowInInspector]
        public abstract T Value { get; }

        public override bool HasValue => Value != null;
    }

    public static class ValueResolver
    {
        public static IValueProvider GetActiveValueSource(
            IValueProvider[] sources,
            MonoBehaviour context
        )
        {
            if (sources == null || sources.Length == 0)
                return null;

            if (!Application.isPlaying)
                return sources[0];

            foreach (var provider in sources)
                if (provider.IsValid)
                    return provider;

            // Debug.LogWarning("condition not met, use default? (last)" + sources[^1], context);
            return null;
        }

        public static IValueProvider<T> GetActiveValueSource<T>(
            IValueProvider<T>[] sources,
            MonoBehaviour context
        )
        {
            if (sources == null || sources.Length == 0)
                return null;

            if (!Application.isPlaying)
                return sources[0];

            foreach (var provider in sources)
                if (provider.IsValid)
                    return provider;

            return null;
        }

        public static bool HasValueProvider(IValueProvider[] sources)
        {
            return sources is { Length: > 0 };
        }
    }
}

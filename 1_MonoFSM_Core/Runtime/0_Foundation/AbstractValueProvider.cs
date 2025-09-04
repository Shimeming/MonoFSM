using MonoFSM.Core.Attributes;
using MonoFSM.Core.DataProvider;
using MonoFSM.Runtime;
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

        public virtual bool IsValid => _conditionGroup.IsValid;
        public abstract bool HasValue { get; }
    }

    public abstract class AbstractValueProvider<T> : AbstractGetter, IValueProvider<T> //提供數值
    {
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
        public static IValueProvider<T> GetActiveValueSource<T>(
            IValueProvider<T>[] sources,
            MonoBehaviour context
        )
        {
            if (sources == null || sources.Length == 0)
                return null;

            foreach (var provider in sources)
                if (provider.IsValid)
                    return provider;

            // Debug.LogWarning("condition not met, use default? (last)" + sources[^1], context);
            return null;
        }

        public static bool HasValueProvider<T>(IValueProvider<T>[] sources)
        {
            return sources is { Length: > 0 };
        }
    }
}

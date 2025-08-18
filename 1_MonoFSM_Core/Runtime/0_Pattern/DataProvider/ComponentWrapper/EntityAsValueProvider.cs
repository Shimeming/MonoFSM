using System;
using MonoFSM.Core.DataProvider;
using MonoFSM.Runtime;
using MonoFSM.Variable.Attributes;
using UnityEngine;

namespace MonoFSM.Core.Runtime
{
    //把MonoBlackboard當成一個值提供者, 才可以被set到varMono上
    public class EntityAsValueProvider : MonoBehaviour, IValueProvider<MonoEntity>
    {
        [CompRef] [Auto] private IEntityProvider _entityProvider;

        public MonoEntity Value => _entityProvider.monoEntity;

        public T GetValue<T>()
        {
            if (typeof(T) != typeof(MonoEntity))
                throw new InvalidOperationException(
                    "GetValue<T>() can only be used with MonoEntity type.");

            return (T)(object)_entityProvider.monoEntity;
        }

        public Type ValueType => typeof(MonoEntity);
        public string Description => ToString();

        public override string ToString()
        {
#if UNITY_EDITOR
            _entityProvider = GetComponent<IEntityProvider>();
            if (_entityProvider == null) return "";
#endif
            return _entityProvider.Description + " as Value";
        }
        //Assign A to B vs
    }
}

using System;
using MonoFSM.Core.Utilities;
using MonoFSM.Variable;
using UnityEngine;

namespace MonoFSM.Core.DataProvider
{
    //讓ValueProvider可以自己拿就好？
    public class ValueRef : AbstractVariableProviderRef
    {
        // [ShowDrawerChain]
        [DropDownRef]
        [SerializeField]
        public AbstractVariableProviderRef _valueProvider;

        public override T1 Get<T1>()
        {
            //FIXME: GC
            var v = ReflectionUtility.GetFieldValueFromPath(
                _valueProvider.objectValue,
                _pathEntries,
                gameObject
            );
            if (v == null)
                // Debug.LogWarning($"ValueRef: Value is null for path '{PropertyPath}'", this);
                return default;

            //string想拿int怎麼辦...
            if (v is T1 value)
            {
                return value;
            }
            else
            {
                //三小也太醜XDDD
                if (typeof(T1) == typeof(string))
                {
                    var str = v.ToString();

                    // Debug.Log($"ValueRef: Converting value to string:{str}, {str.GetType()} -> {typeof(T1)}", this);
                    return (T1)(object)str; //如果是string的話，直接轉成string
                }

                Debug.LogError(
                    $"ValueRef: Value type mismatch. Expected {typeof(T1)}, but got {v?.GetType()}",
                    this
                );
                return default;
            }
            // return _valueProvider.Get<T1>();
        }

        public override Type ValueType =>
            HasFieldPath ? lastPathEntryType : _valueProvider.ValueType;

        public override AbstractMonoVariable VarRaw => _valueProvider?.VarRaw;
        public override VariableTag varTag => _valueProvider?.varTag;

        public override TVariable GetVar<TVariable>()
        {
            if (_valueProvider == null)
            {
                Debug.LogError("ValueRef: _valueProvider is null, cannot get variable.", this);
                return null;
            }

            if (_valueProvider.Get<TVariable>() is { } variable)
                return variable;

            Debug.LogError(
                $"ValueRef: Cannot cast {_valueProvider.objectValue.GetType()} to {typeof(TVariable)}",
                this
            );
            return null;
        }

        public override string Description => _valueProvider?.Description + "." + PropertyPath; //最後一段會重複？
        protected override string DescriptionTag => "ref";
        public override Type GetObjectType => _valueProvider?.ValueType;
    }
}

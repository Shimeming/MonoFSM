using System;
using MonoFSM.Core.Utilities;
using MonoFSM.Variable;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MonoFSM.Core.DataProvider
{
    //讓ValueProvider可以自己拿就好？
    public class ValueRef : AbstractVariableProviderRef, IFieldPathRootTypeProvider
    {
        // [ShowDrawerChain]
        //FIXME: 好像要再整理一下, 為什麼是用這個型別？
        [DropDownRef]
        [SerializeField]
        public AbstractVariableProviderRef _valueProvider;

        public override T1 Get<T1>()
        {
            var (v, info) = ReflectionUtility.GetFieldValueFromPath<T1>(
                StartingObject,
                _pathEntries,
                gameObject
            );
            // if (v == null)
            //     // Debug.LogWarning($"ValueRef: Value is null for path '{PropertyPath}'", this);
            //     return default;

            //string想拿int怎麼辦...
            if (v != null)
            {
                return v;
            }

            //還是有可能是null喔

            // Debug.LogError("GetFieldValueFromPath is null", this);
            // else
            // {
            //     //三小也太醜XDDD
            //     if (typeof(T1) == typeof(string))
            //     {
            //         var str = v.ToString();
            //
            //         // Debug.Log($"ValueRef: Converting value to string:{str}, {str.GetType()} -> {typeof(T1)}", this);
            //         return (T1)(object)str; //如果是string的話，直接轉成string
            //     }
            //
            //     Debug.LogError(
            //         $"ValueRef: Value type mismatch. Expected {typeof(T1)}, but got {v?.GetType()}",
            //         this
            //     );
            //     return default;
            // }
            return default;
            // return _valueProvider.Get<T1>();
        }

        public override Type ValueType =>
            HasFieldPath ? lastPathEntryType : _valueProvider.ValueType;

        public override object StartingObject => _valueProvider?.Get<object>();
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
                $"ValueRef: Cannot cast {_valueProvider.StartingObject.GetType()} to {typeof(TVariable)}",
                this
            );
            return null;
        }

        public override string Description => _valueProvider?.Description + "." + PropertyPath; //最後一段會重複？
        protected override string DescriptionTag => "ref";
        public override Type GetObjectType => _valueProvider?.ValueType;

        public Type GetFieldPathRootType()
        {
            return _valueProvider?.ValueType;
        }
    }
}

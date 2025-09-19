using System;
using MonoFSM.Core.DataProvider;
using MonoFSM.Core.Utilities;
using Object = UnityEngine.Object;

namespace _1_MonoFSM_Core.Runtime._0_Pattern.DataProvider
{
    public class ObjectPropertyProvider : PropertyOfTypeProvider
    {
        public Object _objectInstance;
        protected override string DescriptionTag => "Object Property";

        public override string Description =>
            _objectInstance != null ? _objectInstance.name + "." + PropertyPath : "Null Object";

        public override object StartingObject => _objectInstance;

        public override Type GetObjectType =>
            _objectInstance != null ? _objectInstance.GetType() : typeof(Object);

        public override T1 Get<T1>()
        {
            var (v, info) = ReflectionUtility.GetFieldValueFromPath<T1>(
                StartingObject,
                _pathEntries,
                this
            );
            return v;
        }

        public override Type ValueType => HasFieldPath ? lastPathEntryType : typeof(Object);
    }
}

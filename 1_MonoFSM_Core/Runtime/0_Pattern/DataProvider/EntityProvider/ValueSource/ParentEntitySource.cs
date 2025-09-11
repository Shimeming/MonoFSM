using MonoFSM.Foundation;
using MonoFSM.Runtime;
using UnityEngine;

namespace MonoFSM.Core.Runtime
{
    public class ParentEntitySource : AbstractValueSource<MonoEntity>
    {
        [AutoParent]
        [SerializeField]
        private MonoEntity _parentEntity;
        public override MonoEntity Value => _parentEntity;
    }
}

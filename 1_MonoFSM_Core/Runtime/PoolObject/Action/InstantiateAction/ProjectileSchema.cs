using MonoFSM.Core.Runtime;
using MonoFSM.Variable;
using UnityEngine;

namespace MonoFSM.Core.LifeCycle
{
    public class ProjectileSchema : AbstractEntitySchema
    {
        public Rigidbody _rigidbody;
        public VarVector3Wrapper _initVel;
    }
}

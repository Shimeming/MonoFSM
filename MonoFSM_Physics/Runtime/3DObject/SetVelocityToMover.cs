using MonoFSM.Core.Runtime.Action;
using MonoFSM.Variable;
using MonoFSMCore.Runtime.LifeCycle;

namespace MonoFSM.Core.Runtime.LevelDesign._3DObject
{
    public class SetVelocityToMover : AbstractStateAction
    {
        public SpawnedObjectEntityProvider _providerInParent;
        public VariableTag _aimTransformTag;

        protected override void OnActionExecuteImplement()
        {
            _providerInParent.monoEntity.GetVar<VarTransform>(_aimTransformTag)
                .SetValue(transform, this);
        }
    }
}

//Spawner的模組怎麼做？

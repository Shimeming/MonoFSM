using MonoFSM.Core.Runtime.Action;
using MonoFSM.Runtime.Variable;
using MonoFSMCore.Runtime.LifeCycle;
using UnityEngine;

namespace MonoFSM.Runtime.ObjectPool
{
    public class DespawnAction : AbstractStateAction
    {
        public VarEntity _despawnEntity;

        protected override void OnActionExecuteImplement()
        {
            Debug.Log("DespawnAction: Despawning entity " + _despawnEntity.Value, this);
            _despawnEntity.Value.GetCompCache<MonoObj>().Despawn();
        }
    }
}

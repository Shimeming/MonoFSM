using MonoFSM.Core.Attributes;
using MonoFSM.Core.Runtime.Action;
using MonoFSMCore.Runtime.LifeCycle;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSM.Runtime.ObjectPool
{
    //FIXME: Despawn action?
    /// <summary>
    ///     把自己回收掉
    /// </summary>
    public class ReturnToPoolAction : AbstractStateAction
    {
        [Required]
        [PreviewInInspector]
        [AutoParent]
        private MonoObj _object;

        protected override void OnActionExecuteImplement()
        {
            // Debug.Log("ReturnToPoolAction", this);
            _object.Despawn();
        }
    }
}

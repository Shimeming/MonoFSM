using MonoFSM.Core.Runtime.Action;
using MonoFSM.Runtime.Interact.EffectHit;
using MonoFSM.Variable;
using UnityEngine;

namespace _1_MonoFSM_Core.Runtime.EffectHit.Action
{
    public class EffectHitSetDirAction : AbstractArgEventHandler<GeneralEffectHitData>
    {
        [SerializeField]
        VarVector3 _direction;

        protected override void OnActionExecuteImplement()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnArgEventReceived(GeneralEffectHitData arg)
        {
            var dir = arg.Dir;
            Debug.Log($"EffectHitSetDirAction: Setting direction to dir {dir}", this);
            _direction.SetValue(dir, this);
            // _direction.SetValue(arg.hitNormal ?? Vector3.zero, this);
        }
    }
}

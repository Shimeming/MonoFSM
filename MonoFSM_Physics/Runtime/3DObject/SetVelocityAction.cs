using MonoFSM.Core.Runtime.Action;
using UnityEngine;

namespace MonoFSM.Core.Runtime.LevelDesign._3DObject
{
    public class SetVelocityAction : AbstractStateAction
    {
        [AutoParent] public Rigidbody _rigidbody;
        public Vector3 _velocity;

        protected override void OnActionExecuteImplement()
        {
            SetVelocityToRigidbody(_rigidbody, _velocity);
            // _rigidbody.linearVelocity = _velocity;
        }

        public static void SetVelocityToRigidbody(Rigidbody rigidbody, Vector3 velocity)
        {
            rigidbody.linearVelocity = velocity;
        }
    }
}

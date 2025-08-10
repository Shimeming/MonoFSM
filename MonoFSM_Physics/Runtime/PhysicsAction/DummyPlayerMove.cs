using MonoFSM.Core;
using MonoFSM.Core.Runtime.Action;
using MonoFSM.Variable;
using MonoFSM.Variable.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;
using MonoFSM.Core.Attributes;

namespace MonoFSM_Physics.Runtime.PhysicsAction
{
    public class DummyPlayerMove : AbstractStateAction
    {
        [Required] [CompRef] [AutoChildren] private ICompProvider<Rigidbody> _rigidbodyProvider;
        [Required] [CompRef] [AutoChildren] VarVector2 _axisValueProvider;
        [ShowInDebugMode] Vector2? targetDirection => _axisValueProvider?.CurrentValue;
        [SerializeField] float speed = 5f;

        protected override void OnActionExecuteImplement()
        {
            var rb = _rigidbodyProvider.Get();
            rb.isKinematic = true;
            var targetPosition = rb.transform.position + (Vector3)targetDirection * speed * Time.deltaTime;
            rb.MovePosition(targetPosition);
        }
    }
}

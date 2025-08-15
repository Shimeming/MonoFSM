using MonoFSM.Core.DataProvider;
using MonoFSM.Core.Runtime.Action;
using MonoFSM.Runtime.Attributes;
using UnityEngine;

namespace MonoFSM_Physics.Runtime.PhysicsAction
{
    public class SetRigidbodyKinematicAction : AbstractStateAction
    {
        // [Required] [CompRef] [AutoChildren] private ICompProvider<Rigidbody> _rigidbodyProvider;

        //FIXME: 還是寫code嗎？
        [ValueTypeValidate(typeof(Rigidbody))] [DropDownRef]
        public ValueProvider _rigidbodyValueProvider;
        public bool _isKinematic = true;

        protected override void OnActionExecuteImplement()
        {
            // var rb = _rigidbodyProvider.Get();
            var rb = _rigidbodyValueProvider.Get<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = _isKinematic;
                Debug.Log($"Set Rigidbody Kinematic: {rb.name} to {_isKinematic}", rb.gameObject);
            }
            else
            {
                Debug.LogError("Rigidbody not found in SetRigidbodyKinematicAction", this);
            }
        }
    }
}

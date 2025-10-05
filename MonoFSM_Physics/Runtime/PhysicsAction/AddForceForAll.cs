using MonoFSM.Core.Runtime.Action;
using MonoFSM.Core.Variable;
using MonoFSM.Runtime;
using MonoFSM.Runtime.Variable;
using MonoFSM.Variable;
using MonoFSM.Variable.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSM_Physics.Runtime.PhysicsAction
{
    public interface ForceDirCalculator
    {
        Vector3 CalForce(Rigidbody rb);
    }

    public class AddForceForAll : AbstractStateAction
    {
        [CompRef]
        [AutoChildren(DepthOneOnly = true)]
        ForceDirCalculator _forceDirCalculator;

        protected virtual Vector3 CalForce(Rigidbody rb) //cal force, notonly
        {
            if (_forceDirCalculator != null)
                return _forceDirCalculator.CalForce(rb);
            return _forceDirectionVar.GetValue().normalized * _magnitude;
        }

        public VarListEntity _entities;

        bool HasForceDirCalculator => _forceDirCalculator != null;

        [HideIf("HasForceDirCalculator")]
        public VarVector3 _forceDirectionVar;

        [HideIf("HasForceDirCalculator")]
        [Tooltip("力的大小")]
        [SerializeField]
        private float _magnitude = 10f;

        [SerializeField]
        private float _maxVel = 2f;

        public AddForceAction.ForcePosition _forcePosition = AddForceAction
            .ForcePosition
            .TargetCenterOfMass;

        [Tooltip("力的施加模式")]
        [SerializeField]
        private ForceMode _forceMode = ForceMode.Acceleration;

        protected override void OnActionExecuteImplement()
        {
            var list = _entities.GetList();
            // var force = forceDirection * _magnitude;
            foreach (var entity in list)
            {
                var rb = entity.GetComp<Rigidbody>();
                var force = CalForce(rb);

                //max vel?
                // var targetVel = dir.normalized * _maxVel;
                // Vector3 velChange = targetVel - rb.linearVelocity;
                // var forceToAdd = velChange;
                // force = forceToAdd;

                if (
                    rb.TryGetComponent<ICustomRigidbody>(out var customRigidbody)
                    && !customRigidbody.isPaused
                )
                {
                    customRigidbody.AddForce(force, _forceMode);
                    return;
                }

                Vector3 applicationPoint =
                    _forcePosition == AddForceAction.ForcePosition.TargetCenterOfMass
                        ? rb.worldCenterOfMass
                        : transform.position;
                Debug.Log(
                    "Applying force "
                        + force
                        + " at point "
                        + applicationPoint
                        + " to entity "
                        + entity.name,
                    rb
                );
                DrawArrow.ForDebug(applicationPoint, force, Color.coral, 1f);

                rb.AddForceAtPosition(force, applicationPoint, _forceMode);
            }
        }
    }
}

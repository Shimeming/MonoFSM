using MonoFSM.Core;
using MonoFSM.Core.DataProvider;
using MonoFSM.Variable;
using UnityEngine;

namespace MonoFSM_Physics.Runtime.PhysicsAction
{
    [System.Flags]
    public enum LerpAxis
    {
        None = 0,
        X = 1 << 0,
        Y = 1 << 1,
        Z = 1 << 2,
        XY = X | Y,
        XZ = X | Z,
        YZ = Y | Z,
        All = X | Y | Z,
    }

    //FIXME: 沒有rigidbody不給貼？或是沒有效果？ 自帶isvalid?
    public class LerpToPositionAction : AbstractStateLifeCycleHandler
    {
        // [Required] [CompRef] [AutoChildren] private ICompProvider<Rigidbody> _rigidbodyProvider;
        // [DropDownRef] public ValueProvider _rigidbodyValueProvider;
        public VarComp _rigidbodyVar;
        public Vector3 _offsetPosition = Vector3.zero;
        public LerpAxis _lerpAxis = LerpAxis.All;
        private Rigidbody _rb;

        protected override void OnStateEnter()
        {
            base.OnStateEnter();
            _rb = _rigidbodyVar.Get<Rigidbody>();
            if (_rb == null)
            {
                Debug.LogError("Rigidbody is null. Cannot perform LerpToPositionAction.", this);
                return;
            }
            var rb = _rb;
            rb.isKinematic = true;
            _targetPosition = rb.transform.position + _offsetPosition;
            if (rb.TryGetComponent<ICustomRigidbody>(out var customRigidbody))
                customRigidbody.isPaused = true;
        }

        private Vector3 _targetPosition;

        private Vector3 GetLerpedPosition(Vector3 currentPos, Vector3 targetPos, float deltaTime)
        {
            var lerpPos = currentPos;

            if ((_lerpAxis & LerpAxis.X) != 0)
                lerpPos.x = Mathf.Lerp(currentPos.x, targetPos.x, deltaTime);

            if ((_lerpAxis & LerpAxis.Y) != 0)
            {
                lerpPos.y = Mathf.Lerp(currentPos.y, targetPos.y, deltaTime);
                // Debug.Log(
                //     $"Lerping Y from {currentPos.y} to {targetPos.y} with deltaTime {deltaTime}, result: {lerpPos.y}",
                //     this);
            }

            if ((_lerpAxis & LerpAxis.Z) != 0)
                lerpPos.z = Mathf.Lerp(currentPos.z, targetPos.z, deltaTime);

            return lerpPos;
        }

        //FIXME: 只lerp其中一個軸？ 像是只有y軸，其他不管？

        protected override void OnStateUpdate()
        {
            base.OnStateUpdate();
            if (_rb == null)
                return;
            var lerpPos = GetLerpedPosition(_rb.position, _targetPosition, DeltaTime);
            //character要另外處理...
            if (_rb.TryGetComponent<ICustomRigidbody>(out var customRigidbody))
            {
                // customRigidbody.AddForce((_offsetPosition - rb.position) * 100,
                //     ForceMode.VelocityChange);
                customRigidbody.position = lerpPos;
                return;
            }

            // var rb = _rigidbodyProvider.Get();
            if (_rb == null)
            {
                Debug.LogError("Rigidbody is null. Cannot perform LerpToPositionAction.", this);
                return;
            }

            Debug.Log(
                $"Lerping from {_rb.position} to {_targetPosition} with deltaTime {DeltaTime}, result: {lerpPos}",
                this
            );
            _rb.MovePosition(lerpPos);
        }

        protected override void OnStateExit()
        {
            base.OnStateExit();
            _rb.isKinematic = false;
            if (_rb.TryGetComponent<ICustomRigidbody>(out var customRigidbody))
                customRigidbody.isPaused = false;
        }
    }
}

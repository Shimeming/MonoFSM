using System;
using MonoFSM.Core.Runtime.Action;
using MonoFSM.Runtime.Interact.EffectHit;
using MonoFSM.Variable;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSM.Runtime.PhysicsAction
{
    //GetComponentInParent?

    //FIXME: 不該用這個？應該用findVariableFromOwner?


    //從EffectHitData嗎？ 對象是Rigidbody, 方向
    public class AddTorqueAction : AbstractArgEventHandler<GeneralEffectHitData>
    {
        // [CompRef] [AutoParent] private ICompProvider<Rigidbody> _rigidbodyProvider;
        // [CompRef] [AutoParent] private IHitDataProvider _hitDataProvider;
        public VarComp _rigidbodyVar;
        public VarVector3 _torqueVector;

        // [SerializeField] private Vector3 _torque;
        [SerializeField]
        private float _torqueMagnitude = 10f; // 可以在Inspector中調整

        [SerializeField]
        private ForceMode _forceMode = ForceMode.Impulse;

        //TODO: offset?
        // public void ArgEventReceived(Rigidbody target) //轉型Provider?
        // {
        //     if (target == null) return;
        //     var hitData = _hitDataProvider.GetHitData();
        //     var dir = hitData.Dealer.transform.position - hitData.Receiver.transform.position;
        //     // var _torque = ;
        //     Debug.Log("AddTorqueAction: Applying torque to " + target.name + " with direction: " + dir, this);
        //     Debug.DrawLine(hitData.Dealer.transform.position, hitData.Receiver.transform.position, Color.red, 10f);
        //     target.AddTorque(dir.normalized * _torqueMagnitude, _forceMode);
        // }
        //
        // public void EventReceived<T>(T arg)
        // {
        //     // ArgEventReceived(arg as Rigidbody);
        // }

        //如果沒有額外的，用Receiver
        // protected override void OnActionExecuteImplement()
        // {
        //     var hitData = _hitDataProvider.GetHitData();
        //     if (hitData == null)
        //     {
        //         Debug.LogError("HitData is null in AddTorqueAction", this);
        //         return;
        //     }
        //
        //     if (_rigidbodyProvider == null)
        //     {
        //         Debug.LogError("RigidbodyProvider is not set in AddTorqueAction", this);
        //         return;
        //     }
        //
        //     var target = _rigidbodyProvider.Get();
        //     // var target = hitData.Receiver.transform.GetComponent<Rigidbody>();
        //     if (target == null)
        //     {
        //         Debug.LogError("No Rigidbody found on Receiver in AddTorqueAction", this);
        //         return;
        //     }
        //
        //     ArgEventReceived(target);
        // }


        [Button]
        void TestAction()
        {
            OnActionExecuteImplement();
        }

        protected override void OnActionExecuteImplement()
        {
            Delay();
        }

        private async void Delay()
        {
            try
            {
                await Awaitable.WaitForSecondsAsync(0.1f);
                var bd = _rigidbodyVar.Value as Rigidbody;
                var torqueDir = _torqueVector.Value * _torqueMagnitude;
                // bd.AddTorque(torqueDir, _forceMode);
                bd.AddForce(torqueDir, _forceMode);
                // Debug.Log(
                //     "AddTorqueAction: Applying force to "
                //         + bd.name
                //         + " with direction: "
                //         + torqueDir,
                //     this
                // );
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
        }

        protected override void OnArgEventReceived(GeneralEffectHitData arg)
        {
            //好像拿不到耶？
            var hitData = arg;
            var dir = hitData.Dealer.transform.position - hitData.Receiver.transform.position;

            // var _torque = ;
            // Debug.Log("AddTorqueAction: Applying torque to " + target.name + " with direction: " + dir, this);
            Debug.DrawLine(
                hitData.Dealer.transform.position,
                hitData.Receiver.transform.position,
                Color.red,
                10f
            );
            var bd = _rigidbodyVar.Value as Rigidbody;
            bd.AddTorque(dir.normalized * _torqueMagnitude, _forceMode);
        }
    }
}

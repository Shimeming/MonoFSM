using UnityEngine;

namespace _1_MonoFSM_Core.Runtime.Action.TransformAction
{
    public class GlobalRotationLifeCycleAction : MonoBehaviour
    {
        //FIXME; 必須要放在State下面？

        private void LateUpdate()
            // protected override void OnStateUpdate()
        {
            // base.OnStateUpdate();
            // Debug.Log("GlobalRotationLifeCycleAction OnStateUpdate called", this);
            transform.rotation = Quaternion.identity;
            ;
            // transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.up);
        }

        // private void LateUpdate()
        // {
        //     // 保持 forward 不變，把 up 拉回世界 up
        // }
    }
}
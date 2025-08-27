using MonoFSM.Runtime.Interact.EffectHit;
using MonoFSM.Variable;
using MonoFSMCore.Runtime.LifeCycle;
using UnityEngine;

namespace MonoFSM.Core.LifeCycle
{
    //FIXME: 會很怪嗎？
    public class ShootWithDirectionAfterProcess : MonoBehaviour, IAfterSpawnProcess
    {
        public Transform _directionTransform;
        public float _speed = 10f;

        // [AutoParent] public Playersche _playerDataGroupSchema;
        public VarFloat _modifier;
        public float _minModifier = 0.1f;

        public void AfterSpawn(MonoObj obj, Vector3 position, Quaternion rotation) { }

        public void AfterSpawn(
            MonoObj obj,
            Vector3 position,
            Quaternion rotation,
            GeneralEffectHitData hitData
        )
        {
            var projectileSchema = obj.Entity.GetSchema<ProjectileSchema>();
            var vel = _directionTransform.forward * _speed * (_modifier.Value + _minModifier);
            //第一個frame的速度沒有給到？不可以直接給嗎？ 還是要過一層比較好，set到linearVelocity再給別人處理
            projectileSchema._initVel.SetValue(vel, this);
            //方向和velocity都給了嗎？
            // var rb = projectileSchema._rigidbody;
            // rb.transform.rotation = Quaternion.LookRotation(vel, Vector3.up);
            // rb.linearVelocity = vel; //raycast這邊就要做嗎？

            //還是其實可以把整坨初始化都搞定？ 其實不太懂為什麼原本會錯
            //1. 設定速度
            //2. 設定朝向
            //3. 設定raycast

            //timing? 這樣raycast判定是對的嗎？
            // Debug.Break();
        }
    }
}

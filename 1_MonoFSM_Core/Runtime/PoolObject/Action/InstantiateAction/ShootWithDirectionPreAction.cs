using MonoFSMCore.Runtime.LifeCycle;
using UnityEngine;

namespace MonoFSM.Core.LifeCycle
{
    //FIXME: 會很怪嗎？
    public class ShootWithDirectionPreAction : MonoBehaviour, IPreSpawnAction
    {
        public Transform _directionTransform;
        public float _speed = 10f;

        public void PreSpawn(MonoObj obj, Vector3 position, Quaternion rotation)
        {
            var projectile = obj.GetComponent<ProjectileSchema>();
            projectile._rigidbody.linearVelocity = _directionTransform.forward * _speed;
        }
    }
}

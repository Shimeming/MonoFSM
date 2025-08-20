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

        public void AfterSpawn(MonoObj obj, Vector3 position, Quaternion rotation)
        {

        }

        public void AfterSpawn(MonoObj obj, Vector3 position, Quaternion rotation,
            GeneralEffectHitData hitData)
        {
            var projectileSchema = obj.Entity.GetSchema<ProjectileSchema>();
            // projectile._rigidbody.AddForce(
            //     _directionTransform.forward * _speed * _modifier.Value, ForceMode.VelocityChange);
            projectileSchema._rigidbody.linearVelocity =
                _directionTransform.forward * _speed * (_modifier.Value + _minModifier);
        }
    }
}

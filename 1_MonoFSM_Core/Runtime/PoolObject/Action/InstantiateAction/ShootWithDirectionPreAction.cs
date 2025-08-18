using MonoFSM.Runtime.Interact.EffectHit;
using MonoFSM.Variable;
using MonoFSMCore.Runtime.LifeCycle;
using UnityEngine;

namespace MonoFSM.Core.LifeCycle
{
    //FIXME: 會很怪嗎？
    public class ShootWithDirectionAfterAction : MonoBehaviour, IAfterSpawnAction
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
            var projectile = obj.GetComponent<ProjectileSchema>();
            // projectile._rigidbody.AddForce(
            //     _directionTransform.forward * _speed * _modifier.Value, ForceMode.VelocityChange);
            projectile._rigidbody.linearVelocity =
                _directionTransform.forward * _speed * (_modifier.Value + _minModifier);
        }
    }
}

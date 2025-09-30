using UnityEngine;

namespace MonoFSM_Physics.Runtime.PhysicsAction
{
    public interface ICustomRigidbody //2D, 3D kcc, simpleKinematic都可以包一層？
    {
        public void AddForce(Vector3 force, ForceMode mode);
        public Vector3 position { get; set; } //不一定可以？
        public void Move(Vector3 offset);
        public bool isPaused { set; get; }
    }
}

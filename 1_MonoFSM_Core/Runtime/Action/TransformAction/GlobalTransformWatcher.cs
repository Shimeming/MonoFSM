using Sirenix.OdinInspector;
using UnityEngine;

namespace _1_MonoFSM_Core.Runtime.Action.TransformAction
{
    public class GlobalTransformWatcher : MonoBehaviour
    {
        [ShowInInspector]
        public Vector3 Position => transform.position;

        [ShowInInspector]
        public Quaternion Rotation => transform.rotation;
    }
}

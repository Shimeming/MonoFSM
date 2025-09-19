using System.Collections.Generic;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.Detection;
using MonoFSM.Variable.Attributes;
using UnityEngine;

namespace MonoFSM_Physics.Runtime.Interact.SpatialDetection
{
    //FIXME: 蛤？？
    public class CollisionEventListener : MonoBehaviour
    {
        [CompRef]
        [Auto]
        private Rigidbody _rigidbody;

        private readonly HashSet<CollisionDetectorSource> _registeredDetectors = new();

        public void RegisterDetector(CollisionDetectorSource detectorSource)
        {
            _registeredDetectors.Add(detectorSource);
        }

        private void OnCollisionStay(Collision collision)
        {
            // 直接轉發給所有註冊的detector，讓它們自己處理
            foreach (var detectorSource in _registeredDetectors)
                detectorSource.OnCollisionStay(collision);
        }

        [ShowInDebugMode]
        private GameObject _lastCollisionEnterObj;
    }
}

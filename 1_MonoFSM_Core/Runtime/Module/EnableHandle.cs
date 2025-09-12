using System;
using UnityEngine;

namespace MonoFSM.Core.Module
{
    public class EnableHandle : MonoBehaviour
    {
        public bool _isCachedEnabled;
        public bool _isCachedDisabled;

        public bool isCachedEnabled => _isCachedEnabled;
        public bool isCachedDisabled => _isCachedDisabled;

        private void OnEnable()
        {
            _isCachedEnabled = true;
        }

        private void OnDisable()
        {
            _isCachedDisabled = true;
        }
    }
}

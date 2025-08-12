using System;
using System.Collections.Generic;
using MonoFSM.Core.Detection;
using MonoFSM.Core.Runtime.Interact.SpatialDetection;
using UnityEngine;

namespace MonoFSM.Runtime.Interact.SpatialDetection
{
    public class MouseDetector : BaseDetectProcessor, IDetectionSource
    {
        static MouseDetector _instance;
        private GameObject _clickedObject;
        private bool _mouseClicked;

        public static MouseDetector Instance => _instance;

        private void Awake()
        {
            _instance = this;
        }

        public override IEnumerable<DetectionResult> GetCurrentDetections()
        {
            if (_mouseClicked && _clickedObject != null)
            {
                yield return new DetectionResult(_clickedObject);
            }
        }

        public override void UpdateDetection()
        {
            _mouseClicked = false;
            _clickedObject = null;

            if (Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    _clickedObject = hit.collider.gameObject;
                    _mouseClicked = true;
                }
            }

            ProcessEnterExitEvents();
        }
    }
}

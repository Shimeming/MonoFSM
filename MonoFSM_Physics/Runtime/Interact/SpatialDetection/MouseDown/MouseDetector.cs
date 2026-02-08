using System.Collections.Generic;
using MonoFSM.Core.Detection;
using UnityEngine;

namespace MonoFSM.Runtime.Interact
{
    public class MouseDetector : AbstractDetectionSource
    {
        static MouseDetector _instance;
        private GameObject _clickedObject;
        private bool _mouseClicked;

        public static MouseDetector Instance => _instance;

        private void Awake()
        {
            _instance = this;
        }

        public override List<DetectionResult> GetCurrentDetections()
        {
            _buffer.Clear();
            if (_mouseClicked && _clickedObject != null)
            {
                _buffer.Add(new DetectionResult(_clickedObject));
            }
            return _buffer;
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
                    _thisFrameColliders.Add(_clickedObject.GetComponent<Collider>());
                    // 報告點擊事件給 EffectDetector
                    // ReportEnterEvent(_clickedObject, hit.point, hit.normal);
                }
            }
        }
    }
}

using System.Diagnostics;
using Gizmo;
using UnityEditor;
using UnityEngine;

namespace MonoFSM.Debugging
{
    public static class DebugGizmo
    {
        /// <summary>
        /// Debug想要在Scene看到物理判定的位置/形狀
        /// </summary>
        /// <param name="gobj"></param>
        /// <param name="position">在哪裡噴</param>
        /// <param name="name"></param>
        [Conditional("UNITY_EDITOR")]
        public static void CreateGizmoDebugNode(
            this MonoBehaviour gobj,
            Vector3 position,
            GameObject name
        ) //TODO: 設圖形?? rect, radius?
        {
            var (isLogging, provider) = MonoExtensionLogger.IsLoggingCheck(gobj);
            if (isLogging == false)
                return;
#if UNITY_EDITOR
            var debugAnchor = new GameObject("[DebugAnchor]:" + name);
            debugAnchor.transform.position = position;
            //TODO: gizmo
            //直接掛gizmo marker
            debugAnchor.AddComponent<GizmoMarker>();
            //TODO: 設圖形??

            // debugAnchor.AddComponent<Gizmo
            // Debug.Break();
            EditorGUIUtility.PingObject(debugAnchor);
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public static void DrawLineGizmoNode(
            this MonoBehaviour mono,
            string name,
            Vector3 start,
            Vector3 end,
            Color color
        )
        {
            var provider = mono.GetComponentInParent<DebugProvider>();
            if (provider == null || provider.IsLogInChildren == false)
                return;

            var debugAnchor = new GameObject("[GizmoLine]:" + name);
            debugAnchor.transform.position = start;
            var lineGizmoNode = debugAnchor.AddComponent<LineGizmoNode>();
            lineGizmoNode.offset = end - start;
            lineGizmoNode.color = color;

            // debugAnchor.AddComponent<GizmoMarker>();
            // Debug.DrawLine(start, end, color, 1f);
        }
    }
}

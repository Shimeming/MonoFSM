using UnityEditor;
using UnityEngine;

namespace MonoFSM.EditorExtension
{
    public static class HierarchyDebug
    {
        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            // Register the global event handler for escape key events
            EditorApplication.update += () =>
            {
                //FIXME: 再開一個setting?
                if (IsDebugMode && Application.isPlaying)
                    EditorApplication.RepaintHierarchyWindow();
            };
        }
        private static bool _isDebugMode = false;

        public static bool IsDebugMode
        {
            get => _isDebugMode;
            set
            {
                _isDebugMode = value;
                if (_isDebugMode)
                    Debug.Log("Hierarchy Debug Mode Enabled");
                else
                    Debug.Log("Hierarchy Debug Mode Disabled");
            }
        }
    }
}

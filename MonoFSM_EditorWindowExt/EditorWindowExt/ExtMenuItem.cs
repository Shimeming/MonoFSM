using System.Reflection;
using UnityEditor;

namespace MonoFSM_EditorWindowExt.EditorWindowExt
{
    public static class ExtMenuItem
    {
        [MenuItem("Tools/MonoFSM/Clear Console Log &#_c ", false, 1000)]
        public static void ClearConsoleLog()
        {
            LogEntries.Clear();
        }
    }
}
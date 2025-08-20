using UnityEditor;
using UnityEngine;

public static class EditorCloseWindowTab //Vhierarchy已經有 %W了？
{
    // [MenuItem("RCGs/ShortCut/Close Window Tab %_W")]
    static void CloseTab()
    {
        EditorWindow focusedWindow = EditorWindow.focusedWindow;
        if (focusedWindow != null)
        {
            // Debug.Log(focusedWindow is MonoNodeWindow);
            // Debug.Log(focusedWindow.GetType());
            // if (focusedWindow is MonoNodeWindow)
            // CloseTab(focusedWindow);
            focusedWindow.Close();
        }
        else
        {
            Debug.LogWarning("Found no focused window to close");
        }
    }
}

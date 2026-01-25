using System.Collections.Generic;
using MonoDebugSetting;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace UnityToolbarExtender.ToolbarElements
{
    [InitializeOnLoad]
    public static class ToolbarDebugMode
    {
        private const string ElementId = "MonoFSM/DebugMode";

        static ToolbarDebugMode()
        {
            RuntimeDebugSetting.OnDebugModeChanged -= OnDebugModeChanged;
            RuntimeDebugSetting.OnDebugModeChanged += OnDebugModeChanged;
        }

        private static void OnDebugModeChanged(bool value)
        {
            MainToolbar.Refresh(ElementId);
        }

        [MainToolbarElement(ElementId, defaultDockPosition = MainToolbarDockPosition.Right)]
        static IEnumerable<MainToolbarElement> CreateDebugModeToggle()
        {
            yield return new MainToolbarToggle(
                GetContent(),
                RuntimeDebugSetting.IsDebugMode,
                OnToggle);
        }

        private static MainToolbarContent GetContent()
        {
            bool isDebug = RuntimeDebugSetting.IsDebugMode;
            return new MainToolbarContent
            {
                text = isDebug ? "DEBUG" : "Normal",
                tooltip = isDebug ? "Debug Mode ON - Click to disable" : "Debug Mode OFF - Click to enable",
                image = EditorGUIUtility.IconContent(isDebug ? "d_DebuggerEnabled" : "d_DebuggerDisabled").image as Texture2D
            };
        }

        private static void OnToggle(bool value)
        {
            RuntimeDebugSetting.SetDebugMode(value);
        }
    }
}

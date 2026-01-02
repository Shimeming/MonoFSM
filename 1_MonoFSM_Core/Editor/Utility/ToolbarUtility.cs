using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MonoFSM.Utility.Editor;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace Example
{
    [InitializeOnLoad]
    public static partial class ToolbarUtility
    {
        // PRIVATE MEMBERS

        private static ScriptableObject _mainToolbar;
        private static int _mainToolbarInstanceID;
        private static VisualElement _leftToolbar;
        private static VisualElement _rightToolbar;
        private static string _currentScene;
        private static string[] _scenePaths;
        private static string[] _sceneNames;

        // CONSTRUCTORS

        //FIXME: 被 Unity改掉了
        static ToolbarUtility()
        {
            // EditorApplication.update -= Update;
            // EditorApplication.update += Update;
        }

        // PUBLIC METHODS

        public static void InvalidateToolbar()
        {
            _mainToolbarInstanceID += 1;
        }

        // PARTIAL METHODS

        static partial void OnUpdate();

        static partial void OnLeftToolbarAttached(VisualElement toolbar);

        static partial void OnRightToolbarAttached(VisualElement toolbar);

        // PRIVATE METHODS

        private static void Update()
        {
            if (_scenePaths == null || _scenePaths.Length != EditorBuildSettings.scenes.Length)
            {
                var scenePaths = new List<string>();
                var sceneNames = new List<string>();

                foreach (var scene in EditorBuildSettings.scenes)
                {
                    if (scene.path == null || scene.path.StartsWith("Assets") == false)
                        continue;

                    var scenePath = Application.dataPath + scene.path.Substring(6);

                    scenePaths.Add(scenePath);
                    sceneNames.Add(Path.GetFileNameWithoutExtension(scenePath));
                }

                _scenePaths = scenePaths.ToArray();
                _sceneNames = sceneNames.ToArray();

                InvalidateToolbar();
            }

            var currentScene = EditorSceneManager.GetActiveScene().name;
            if (_currentScene != currentScene)
            {
                _currentScene = currentScene;

                InvalidateToolbar();
            }

            if (_mainToolbar == null)
            {
                var toolbars = Resources.FindObjectsOfTypeAll(
                    typeof(Editor).Assembly.GetType("UnityEditor.Toolbar")
                );
                _mainToolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;
            }

            if (_mainToolbar != null)
            {
                var mainToolbarInstanceID = _mainToolbar.GetInstanceID();
                if (mainToolbarInstanceID != _mainToolbarInstanceID)
                {
                    _mainToolbarInstanceID = mainToolbarInstanceID;

                    RefreshToolbar(
                        ref _leftToolbar,
                        "ToolbarZoneLeftAlign",
                        FlexDirection.Row,
                        LeftToolbarAttached
                    );
                    RefreshToolbar(
                        ref _rightToolbar,
                        "ToolbarZoneRightAlign",
                        FlexDirection.RowReverse,
                        RightToolbarAttached
                    );
                }
            }

            OnUpdate();
        }

        private static void RefreshToolbar(
            ref VisualElement toolbar,
            string toolbarID,
            FlexDirection direction,
            Action<VisualElement> onAttachToMainToolbar
        )
        {
            if (toolbar != null)
            {
                toolbar.RemoveFromHierarchy();
                toolbar = null;
            }

            var root = _mainToolbar
                .GetType()
                .GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
            if (root != null)
            {
                var rawRoot = root.GetValue(_mainToolbar);
                if (rawRoot != null)
                {
                    var toolbarRoot = rawRoot as VisualElement;
                    var toolbarZone = toolbarRoot.Q(toolbarID);

                    toolbar = new VisualElement
                    {
                        style = { flexGrow = 1, flexDirection = direction },
                    };

                    toolbar.Add(new VisualElement { style = { flexGrow = 1 } });

                    toolbarZone.Add(toolbar);

                    onAttachToMainToolbar(toolbar);
                }
            }
        }

        private static void LeftToolbarAttached(VisualElement toolbar)
        {
            OnLeftToolbarAttached(toolbar);
        }

        private static void RightToolbarAttached(VisualElement toolbar)
        {
            OnRightToolbarAttached(toolbar);

            //			toolbar.Add(CreateToolbarButton(ToggleTracing, "d_UnityEditor.ConsoleWindow@2x", "Tracing", GetTracingColor()));

            toolbar.Add(CreateToolbarButton(ShowPackageManager, "Package Manager", "Packages"));
            toolbar.Add(CreateToolbarButton(ShowSettings, "Settings", "Settings"));
            //			toolbar.Add(CreateToolbarButton(ShowRunnerControls, "d_SceneViewCamera@2x", "Fusion Controls"));
            toolbar.Add(
                CreateToolbarButton(ShowFusionConfig, "ScriptableObject Icon", "Fusion Config")
            );
            toolbar.Add(
                CreateToolbarButton(
                    ShowScenes,
                    "BuildSettings.Editor",
                    string.IsNullOrEmpty(_currentScene) == false ? _currentScene : "Scene"
                )
            );
        }

        // 		private static Color GetTracingColor()
        // 		{
        // #if UNITY_2023_1_OR_NEWER
        // 			PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup), out string[] scriptingDefineSymbols);
        // #else
        // 			PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, out string[] scriptingDefineSymbols);
        // #endif
        // 			if (scriptingDefineSymbols == null)
        // 				return default;
        //
        // 			for (int i = 0; i < scriptingDefineSymbols.Length; ++i)
        // 			{
        // 				if (scriptingDefineSymbols[i] == KCC.TRACING_SCRIPT_DEFINE)
        // 					return Color.green;
        // 			}
        //
        // 			return default;
        // 		}

        // 		private static void ToggleTracing()
        // 		{
        // #if UNITY_2023_1_OR_NEWER
        // 			PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup), out string[] scriptingDefineSymbols);
        // #else
        // 			PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, out string[] scriptingDefineSymbols);
        // #endif
        // 			if (scriptingDefineSymbols == null)
        // 			{
        // #if UNITY_2023_1_OR_NEWER
        // 				PlayerSettings.SetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup), new string[] { KCC.TRACING_SCRIPT_DEFINE });
        // #else
        // 				PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, new string[] { KCC.TRACING_SCRIPT_DEFINE });
        // #endif
        // 				return;
        // 			}
        //
        // 			bool addDefine = true;
        //
        // 			List<string> newScriptingDefineSymbols = new List<string>(scriptingDefineSymbols);
        // 			for (int i = newScriptingDefineSymbols.Count - 1; i >= 0; --i)
        // 			{
        // 				if (newScriptingDefineSymbols[i] == KCC.TRACING_SCRIPT_DEFINE)
        // 				{
        // 					newScriptingDefineSymbols.RemoveAt(i);
        // 					addDefine = false;
        // 				}
        // 			}
        //
        // 			if (addDefine == true)
        // 			{
        // 				newScriptingDefineSymbols.Add(KCC.TRACING_SCRIPT_DEFINE);
        // 			}
        // #if UNITY_2023_1_OR_NEWER
        // 			PlayerSettings.SetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup), newScriptingDefineSymbols.ToArray());
        // #else
        // 			PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newScriptingDefineSymbols.ToArray());
        // #endif
        // 		}

        private static void ShowScenes()
        {
            var menu = new GenericMenu();

            string currentPrefix = default;

            for (var i = 0; i < _sceneNames.Length; ++i)
            {
                var scenePath = _scenePaths[i];
                if (scenePath != null)
                {
                    var prefixIndex = scenePath.IndexOf("/Assets/Example/");
                    if (prefixIndex >= 0)
                    {
                        var prefix = scenePath.Substring(prefixIndex, 18);
                        if (currentPrefix != default && currentPrefix != prefix)
                            menu.AddSeparator("");

                        currentPrefix = prefix;
                    }
                }

                var sceenName = _sceneNames[i];

                menu.AddItem(
                    new GUIContent(sceenName),
                    sceenName == _currentScene,
                    sceneIndex =>
                    {
                        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        {
                            EditorSceneManager.OpenScene(
                                _scenePaths[(int)sceneIndex],
                                OpenSceneMode.Single
                            );
                            InvalidateToolbar();
                        }
                    },
                    i
                );
            }

            menu.ShowAsContext();
        }

        private static void ShowFusionConfig()
        {
            EditorApplication.ExecuteMenuItem("Tools/Fusion/Network Project Config");
        }

        private static void ShowRunnerControls()
        {
            EditorApplication.ExecuteMenuItem("Tools/Fusion/Windows/Network Runner Controls");
        }

        private static void ShowSettings()
        {
            var menu = new GenericMenu();
            menu.AddItem(
                new GUIContent("Project Settings"),
                false,
                () => EditorApplication.ExecuteMenuItem("Edit/Project Settings...")
            );
            menu.AddItem(
                new GUIContent("Preferences/Settings"),
                false,
                () => SettingsService.OpenUserPreferences("Preferences/General")
            ); //EditorApplication.ExecuteMenuItem("Edit/Preferences..."));
            menu.AddSeparator("");
            menu.AddItem(
                new GUIContent("Realtime Settings"),
                false,
                () => EditorApplication.ExecuteMenuItem("Tools/Fusion/Realtime Settings")
            );
            menu.AddItem(
                new GUIContent("Network Project Config"),
                false,
                () => EditorApplication.ExecuteMenuItem("Tools/Fusion/Network Project Config")
            );
            menu.ShowAsContext();
        }

        private static void ShowPackageManager()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Package Manager"), false, () => Window.Open(""));

            menu.AddItem(
                new GUIContent("Git Dependency Installer"),
                false,
                () =>
                {
                    GitDependencyWindow.ShowWindow();
                }
            );
            menu.AddSeparator("");
            menu.AddItem(
                new GUIContent("Git Dependency Version Check"),
                false,
                GitDependencyVersionChecker.ManualVersionCheck
            );

            menu.ShowAsContext();
        }

        private static VisualElement CreateToolbarButton(
            Action onClick,
            string icon = null,
            string text = null,
            Color color = default
        )
        {
            var buttonElement = new Button(onClick);
            buttonElement.AddToClassList("unity-toolbar-button");
            buttonElement.AddToClassList("unity-editor-toolbar-element");
            buttonElement.RemoveFromClassList("unity-button");
            buttonElement.style.marginRight = 2;
            buttonElement.style.marginLeft = 2;

            if (color != default)
                buttonElement.style.color = color;

            if (string.IsNullOrEmpty(icon) == false)
            {
                var iconElement = new VisualElement();
                iconElement.AddToClassList("unity-editor-toolbar-element__icon");
                iconElement.style.backgroundImage = Background.FromTexture2D(
                    EditorGUIUtility.IconContent(icon).image as Texture2D
                );
                buttonElement.Add(iconElement);
            }

            if (string.IsNullOrEmpty(text) == false)
            {
                var textElement = new TextElement();
                textElement.text = text;
                textElement.style.marginLeft = 4;
                textElement.style.marginRight = 4;
                textElement.style.unityTextAlign = TextAnchor.MiddleCenter;
                buttonElement.Add(textElement);
            }

            return buttonElement;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using static MonoFSMEditor.RefectionUtility;

namespace MonoFSM.Editor.AnimationWindow
{
    public static class AnimationWindowSearchBar
    {
        [MenuItem("MonoFSM/Edit Animation of State %_E")]
        static void OpenAnimationWindow()
        {
            EditorApplication.ExecuteMenuItem("Window/Animation/Animation");
            if (Application.isPlaying) return;
            if (Selection.activeGameObject == null) return;

            //去找animator play action來播？好像也沒什麼必要
            var selection = Selection.activeGameObject;
            if (selection != null)
            {
                var animator = selection.GetComponentInChildren<Animator>();
                if(animator != null)
                    Selection.activeGameObject = animator.gameObject;
            }
            //FIXME: iAnimatorPlayAction?
            
            // var iAnimatorPlayAction = Selection.activeGameObject.GetComponentInChildren<IAnimatorPlayAction>();
            // if (iAnimatorPlayAction != null)
            // {
            //     Debug.Log("[ShortCut] Edit anim of state" + iAnimatorPlayAction);
            //     _lastEditState = iAnimatorPlayAction;
            //     AnimatorHelper.EditClip(_lastEditState.BindAnimator, _lastEditState.Clip);
            // }
        }
        
        
        private static Dictionary<EditorWindow, AnimationWindowNavbar> navbars_byWindow = new();
        private static Type t_AnimationWindow;
        private static Type t_HostView;
        private static Type t_EditorWindowDelegate;
        private static MethodInfo mi_WrappedGUI;
        
        static AnimationWindowSearchBar()
        {
            t_AnimationWindow = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.AnimationWindow");
            t_HostView = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.HostView");
            t_EditorWindowDelegate = t_HostView.GetNestedType("EditorWindowDelegate", maxBindingFlags);
            mi_WrappedGUI = typeof(AnimationWindowSearchBar).GetMethod(nameof(WrappedGUI), maxBindingFlags);
        }

        [InitializeOnLoadMethod]
        private static void Init()
        {
            EditorApplication.update -= CheckForAnimationWindows;
            EditorApplication.update += CheckForAnimationWindows;
            
            EditorApplication.delayCall -= DelayCallLoop;
            EditorApplication.delayCall += DelayCallLoop;
        }

        private static void CheckForAnimationWindows()
        {
            var animationWindows = Resources.FindObjectsOfTypeAll(t_AnimationWindow).Cast<EditorWindow>();
            
            foreach (var window in animationWindows)
            {
                if (window != null && window.hasFocus)
                {
                    UpdateGUIWrapping(window);
                }
            }
        }

        private static void DelayCallLoop()
        {
            var animationWindows = Resources.FindObjectsOfTypeAll(t_AnimationWindow).Cast<EditorWindow>();
            
            foreach (var window in animationWindows)
            {
                if (window != null)
                {
                    UpdateGUIWrapping(window);
                }
            }

            EditorApplication.delayCall -= DelayCallLoop;
            EditorApplication.delayCall += DelayCallLoop;
        }

        private static void WrappedGUI(EditorWindow window)
        {
            var navbarHeight = 26;

            void navbarGui()
            {
                if (!navbars_byWindow.ContainsKey(window))
                    navbars_byWindow[window] = new AnimationWindowNavbar(window);

                var navbarRect = window.position.SetPos(0, 0).SetHeight(navbarHeight);
                navbars_byWindow[window].OnGUI(navbarRect);
            }

            void defaultGuiWithOffset()
            {
                var topOffset = navbarHeight;
                var m_Pos_original = window.GetFieldValue<Rect>("m_Pos");

                GUI.BeginGroup(m_Pos_original.SetPos(0, 0).AddHeightFromBottom(-topOffset));
                window.SetFieldValue("m_Pos", m_Pos_original.AddHeightFromBottom(-topOffset));

                // 在調用原始OnGUI之前，先處理navbar的dropdown事件
                if (navbars_byWindow.ContainsKey(window))
                {
                    var navbar = navbars_byWindow[window];
                    navbar.HandleDropdownEventsFirst();
                }

                try
                {
                    // 調用原本的OnGUI邏輯
                    window.InvokeMethod("OnGUI");
                }
                catch (Exception exception)
                {
                    if (exception.InnerException is ExitGUIException)
                        throw exception.InnerException;
                    else
                        throw exception;
                }

                window.SetFieldValue("m_Pos", m_Pos_original);
                GUI.EndGroup();
            }

            // var doNavbarFirst = navbars_byWindow.ContainsKey(window) && navbars_byWindow[window].isSearchActive;

            // 恢復原來的順序，但優化控制項ID管理
            defaultGuiWithOffset();
            navbarGui();

            // 最後繪製任何需要在最上層的dropdown
            // if (navbars_byWindow.ContainsKey(window))
            // {
            //     var navbar = navbars_byWindow[window];
            //     // if (navbar.shouldDrawDropdown)
            //     // {
            //     //     navbar.shouldDrawDropdown = false;
            //     //     navbar.DrawDropdownOverlay();
            //     // }
            // }
        }

        private static void UpdateGUIWrapping(EditorWindow window)
        {
            if (!window || !window.hasFocus) return;
            if (window.GetType() != t_AnimationWindow) return;
            // 清除原有的OnGUI方法，避免重複調用
            var curOnGUIMethod = window.GetMemberValue("m_Parent")?.GetMemberValue<System.Delegate>("m_OnGUI")?.Method;
            if (curOnGUIMethod == null) return;

            var isWrapped = curOnGUIMethod == mi_WrappedGUI;
            var shouldBeWrapped = true; // 總是啟用search bar

            void wrap()
            {
                var hostView = window.GetMemberValue("m_Parent");
                if (hostView == null) return;

                var newDelegate = mi_WrappedGUI.CreateDelegate(t_EditorWindowDelegate, window);
                hostView.SetMemberValue("m_OnGUI", newDelegate);
                window.Repaint();
            }

            void unwrap()
            {
                var hostView = window.GetMemberValue("m_Parent");
                if (hostView == null) return;

                var originalDelegate = hostView.InvokeMethod("CreateDelegate", "OnGUI");
                hostView.SetMemberValue("m_OnGUI", originalDelegate);
                window.Repaint();
            }

            if (shouldBeWrapped && !isWrapped)
                wrap();

            if (!shouldBeWrapped && isWrapped)
                unwrap();
        }
    }
}
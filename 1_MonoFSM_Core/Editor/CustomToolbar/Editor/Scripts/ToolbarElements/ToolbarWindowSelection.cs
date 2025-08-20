using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace UnityToolbarExtender.ToolbarElements
{
    public class MySelector : OdinSelector<Type>
    {
        private readonly List<Type> source;
        private readonly bool supportsMultiSelect;

        public MySelector(List<Type> source, bool supportsMultiSelect)
        {
            this.source = source;
            this.supportsMultiSelect = supportsMultiSelect;
        }

        protected override void BuildSelectionTree(OdinMenuTree tree)
        {
            tree.Config.DrawSearchToolbar = true;
            tree.Selection.SupportsMultiSelect = this.supportsMultiSelect;
            tree.Config.SelectMenuItemsOnMouseDown = true;
            tree.Config.ConfirmSelectionOnDoubleClick = true;

            foreach (var itemType in source)
            {
                tree.Add(itemType.Name, itemType);
            }
            // tree.Add("Defaults/None", null);
            // tree.Add("Defaults/A", new EditorWindow());
            // tree.Add("Defaults/B", new EditorWindow());

            // tree.AddRange(this.source, x => x.Path, x => x.SomeTexture);
        }

        [OnInspectorGUI]
        private void DrawInfoAboutSelectedItem()
        {
            Type selected = this.GetCurrentSelection().FirstOrDefault();

            if (selected != null)
            {
                // GUILayout.Label("Name: " + selected.Name);
                // GUILayout.Label("Data: " + selected.Data);
            }
        }
    }


    //FIXME: 應該要可以把這個抽出去，任何繼承BaseToolbarElement
    [Serializable]
    internal class ToolbarWindowSelection : BaseToolbarElement
    {
        public ToolbarWindowSelection()
        {
        }

        public override string NameInList => "[Button] Tool Window";
        private static GUIContent toolWindowBtn;
        private static GUIContent _pasteLinkBtn;

        public override void Init()
        {
            toolWindowBtn = EditorGUIUtility.IconContent("Settings@2x");
            toolWindowBtn.tooltip = "Open Tool Window";
            _pasteLinkBtn = EditorGUIUtility.IconContent("Clipboard");
            _pasteLinkBtn.tooltip = "Paste link";

        }

        protected override void OnDrawInList(Rect position)
        {

        }


        protected override void OnDrawInToolbar()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                normal =
                {
                    textColor = new Color(0.2f, 0.5f, 0.1f)
                },
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter
            };
            if (EditorWindow.focusedWindow)
            {
                var labelContent = new GUIContent(EditorWindow.focusedWindow.titleContent.text, "Focusing Window");
                GUILayout.Label(labelContent, style);
                //repaint?
            }
            //偷懶了...每個工具都要寫一個BaseToolbarElement很麻煩，直接在這裡寫
            if (GUILayout.Button(_pasteLinkBtn, ToolbarStyles.commandButtonStyle))
            {
            }

            //偷懶放label
            //FIXME: 改名會爛掉
            var isDebugMode = EditorPrefs.GetBool("MonoFSM.DebugSetting.IsDebugMode", false);

            if (isDebugMode)
            {

                //green text

                // style.padding = new RectOffset(0, 0, 0, 0);
                // style.margin = new RectOffset(0, 0, 0, 0);
                // style.fontStyle = FontStyle.Bold;
                // style.normal.background = null;
                GUILayout.Label("Debug Mode", style);
            }




        }
    }
}

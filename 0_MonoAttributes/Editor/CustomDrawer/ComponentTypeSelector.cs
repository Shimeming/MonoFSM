using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace MonoFSM.Core
{
    //搜尋所有繼承的Component
    public class ComponentTypeSelector : OdinSelector<Type>
    {
        private Type filterType;

        public ComponentTypeSelector(Type _fileterType)
        {
            filterType = _fileterType;
            DrawConfirmSelectionButton = true;
        }

        protected override void BuildSelectionTree(OdinMenuTree tree)
        {
            tree.Config.DrawSearchToolbar = true;

            // tree.Selection.SupportsMultiSelect = this.supportsMultiSelect;

            var types = filterType.FilterSubClassOrImplementationFromDomain();
            foreach (var type in types)
            {
                // Filter out types with Obsolete attribute
                if (type.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length > 0)
                    continue;

                tree.Add(type.Name, type);
            }

            tree.Config.SelectMenuItemsOnMouseDown = true;
            tree.Config.ConfirmSelectionOnDoubleClick = true;
        }

        [OnInspectorGUI]
        private void DrawInfoAboutSelectedItem() //單點後，額外顯示
        {
            Type selected = this.GetCurrentSelection().FirstOrDefault();

            if (selected != null)
            {
                GUILayout.Label("Selected: " + selected.Name);

                // GUILayout.Label("Data: " + selected.Data);
            }
        }

        public void EnableSingleClickToConfirm()
        {
            SelectionTree.EnumerateTree(x =>
            {
                x.OnDrawItem -= EnableSingleClickToConfirm;
                x.OnDrawItem += EnableSingleClickToConfirm;
            });
        }

        private void EnableSingleClickToConfirm(OdinMenuItem obj)
        {
            var type = Event.current.type;
            if (type == EventType.Layout || !obj.Rect.Contains(Event.current.mousePosition))
                return;
            GUIHelper.RequestRepaint();

            // if (Event.current.type == UnityEngine.EventType.MouseDrag && obj is T && this.IsValidSelection(Enumerable.Repeat<T>((T) obj.Value, 1)))
            //     obj.Select();
            if (type != EventType.MouseUp || obj.ChildMenuItems.Count != 0)
                return;
            obj.Select();
            Debug.Log("ConfirmSelection" + obj.Name);
            obj.MenuTree.Selection.ConfirmSelection();
            Event.current.Use();
        }
    }
}

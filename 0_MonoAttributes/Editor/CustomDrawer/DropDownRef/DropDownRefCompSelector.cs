using System;
using System.Linq;
using MonoFSM.CustomAttributes;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MonoFSM.Core
{
    //FIXME: 如果也有帶ValueTypeValidate，可以過濾更細
    public class DropDownRefCompSelector : OdinSelector<Component>
    {
        private Type _filterType;
        private Component _forComp;

        // private Type _parentType;
        DropDownRefAttribute _attribute;

        public DropDownRefCompSelector(
            Component forComp,
            Type filterType,
            DropDownRefAttribute attribute
        )
        {
            if (forComp == null)
                throw new ArgumentNullException(nameof(forComp));
            _forComp = forComp;
            _filterType = filterType;
            DrawConfirmSelectionButton = true;
            _attribute = attribute;
            // _parentType = _attribute._parentType;
        }

        protected override void BuildSelectionTree(OdinMenuTree tree)
        {
            tree.Config.DrawSearchToolbar = true;

            // tree.Selection.SupportsMultiSelect = this.supportsMultiSelect;
            var parentType = _attribute._parentType ?? typeof(IDropdownRoot); //FIXME: 不太喜歡...UI也想撈
            Component[] comps;
            // parentType ??=

            //只找當前parent下的所有_filterType component
            if (_attribute._findFromParentTransform)
            {
                var parent = _forComp.transform.parent;
                if (parent == null)
                {
                    Debug.LogError("Parent is null");
                    return;
                }
                comps = parent.GetComponentsInChildren(_filterType, true);
            }
            //1. prefab裏直接找root下的所有_filterType component
            else if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                //FIXME: 行為不一致！？
                var root = PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot;
                comps = root.GetComponentsInChildren(_filterType, true);
            }
            else
            {
                Debug.Log("ParentType is " + parentType + " filterType is " + _filterType);
                comps = _forComp.GetComponentsOfSiblingAll(parentType, _filterType);
            }

            if (comps == null || comps.Length == 0)
            {
                Debug.LogError(
                    "No components found of type "
                        + _filterType.Name
                        + " in parent of "
                        + _forComp.name
                );
                return;
            }
            // var types = filterType.FilterSubClassOrImplementationFromDomain();
            foreach (var comp in comps)
            {
                // 排除當前組件本身，避免遞迴引用
                if (comp == _forComp)
                    continue;

                //FIXME: 好亂：應該要和上面邏輯共用？
                var parents = comp.GetComponentsInParent<IDropdownRoot>(true);
                if (parents == null || parents.Length == 0)
                {
                    Debug.LogError("IDropdownRoot not found for component " + comp.name, comp);
                    continue;
                }

                //FIXME: 可以有多層？
                var ownerNames = new string[parents.Length];
                for (var i = parents.Length - 1; i >= 0; i--) // 从最远的父级开始，构建层级路径
                    ownerNames[parents.Length - 1 - i] = parents[i].name;
                var ownerPath = string.Join("/", ownerNames);
                var items = tree.Add(
                    ownerPath + "/" + comp.name + " (" + comp.GetType().Name + ")",
                    comp
                );
                foreach (var item in items)
                    item.DefaultToggledState = false;
                // Debug.Log("Add type " + comp.GetType() + " ownerName is " + ownerName);
            }
            tree.Config.SelectMenuItemsOnMouseDown = true;
            tree.Config.ConfirmSelectionOnDoubleClick = true;
        }

        [OnInspectorGUI]
        private void DrawInfoAboutSelectedItem() //單點後，額外顯示
        {
            var selected = GetCurrentSelection().FirstOrDefault();

            if (selected != null)
                GUILayout.Label("Selected: " + selected.name);
            // GUILayout.Label("Data: " + selected.Data);
        }

        //FIXME: 單點選擇後���自動確認選擇...hack code
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
            // Debug.Log("ConfirmSelection" + obj.Name);
            obj.MenuTree.Selection.ConfirmSelection();

            Event.current.Use();
        }
    }
}


// {
//         // private readonly List<AbstractStateAction> source;
//         private readonly bool supportsMultiSelect;
//
//         public StateActionSelector(Type baseType, bool supportsMultiSelect)
//         {
//             // this.source = source;
//             this.supportsMultiSelect = supportsMultiSelect;
//         }
//
//         protected override void BuildSelectionTree(OdinMenuTree tree)
//         {
//             tree.Config.DrawSearchToolbar = true;
//             tree.Selection.SupportsMultiSelect = this.supportsMultiSelect;
//
//             var types = typeof(AbstractStateAction).FilterSubClassFromDomain();
//             foreach (var type in types)
//             {
//                 tree.Add(type.Name, type);
//             }
//             // tree.Add("Defaults/A", new AbstractStateAction());
//             // tree.Add("Defaults/B", new AbstractStateAction());
//
//             // tree.AddRange(this.source, x => x.Path, x => x.SomeTexture);
//         }
//
//         [OnInspectorGUI]
//         private void DrawInfoAboutSelectedItem()
//         {
//             Type selected = this.GetCurrentSelection().FirstOrDefault();
//
//             if (selected != null)
//             {
//                 GUILayout.Label("Name: " + selected.Name);
//                 // GUILayout.Label("Data: " + selected.Data);
//             }
//         }
//     }
// }

using System.Collections;
using System;
using MonoFSM.Core.Attributes;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MonoFSM.Core
{
    [DrawerPriority(0, 1, 0.25)]
    public class SOConfigAttributeDrawer : OdinAttributeDrawer<SOConfigAttribute>
    {

        /// <summary>
        /// 取得可用的路徑選項
        /// </summary>
        private List<string> GetAvailablePaths()
        {
            var paths = new List<string> { "Assets" };
            var localPackages = MonoFSM.Core.PackageHelper.GetLocalPackagePaths();
            paths.AddRange(localPackages);
            return paths;
        }

        /// <summary>
        /// 使用路徑配置建立 ScriptableObject
        /// </summary>
        private ScriptableObject CreateScriptableObjectWithSelectedPath(Type configType, string defaultFileName)
        {
            var config = SOPathSettingConfig.Instance;

            // 使用新的分離式 API
            var basePath = config.GetBasePathForType(configType);
            var relativePath = config.GetRelativePathForType(configType, Attribute.SubFolderPath);

            // 建立 ScriptableObject
            return configType.CreateScriptableObjectAt(basePath, relativePath, defaultFileName);
        }
        private void CreateSOForSO()
        {
            var configType = Property.ValueEntry.TypeOfValue;
            var sObj = Property.ParentValues[0] as ScriptableObject;
            
            var fileName = $"[{sObj.name}]_{configType.Name}.asset";
            var myScriptableObject = CreateScriptableObjectWithSelectedPath(configType, fileName);

            Property.ValueEntry.WeakSmartValue = myScriptableObject;
        }

        private void CreateSOForMonoBehavior()
        {
            var configType = Property.ValueEntry.TypeOfValue;
            var parentComp = Property.ParentValues[0] as Component;

            string fileName;
            if (parentComp)
            {
                var gObj = parentComp.gameObject;
                fileName = $"{gObj.name}_{configType.Name}.asset";
            }
            else
            {
                fileName = $"0_{configType.Name}_{Property.Name}.asset";
            }

            var myScriptableObject = CreateScriptableObjectWithSelectedPath(configType, fileName);
            Property.ValueEntry.WeakSmartValue = myScriptableObject;

            // 執行後處理方法
            if (!string.IsNullOrEmpty(Attribute.PostProcessMethodName))
            {
                Debug.Log("PostProcessMethodName: " + Attribute.PostProcessMethodName);
                Debug.Log("parentComp: " + parentComp, parentComp);
                var type = parentComp.GetType();
                var method = type.GetMethod(Attribute.PostProcessMethodName,
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);
                if (method != null)
                    method.Invoke(parentComp, new object[] { });
                else
                    Debug.LogError("PostProcessMethodName not found: " + Attribute.PostProcessMethodName);
            }
        }

        private void DrawCreateButtonForList()
        {
            var guiContent =
                new GUIContent("Add DescriptableTag", null, "Create a new ScriptableObject and add to list");

            var buttonClicked = SirenixEditorGUI.SDFIconButton(
                EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight),
                guiContent,
                SdfIconType.FileEarmarkPlus,
                IconAlignment.LeftEdge);

            if (buttonClicked) CreateSOForList();
        }

        private void CreateSOForList()
        {
            // 取得 List 的元素類型
            var listType = Property.ValueEntry.TypeOfValue;
            var elementType = listType.GetGenericArguments()[0];

            // 產生檔案名稱
            string fileName;
            if (Property.ParentValues[0] is ScriptableObject sObj)
            {
                fileName = $"New {sObj.name}_{elementType.Name}.asset";
            }
            else if (Property.ParentValues[0] is Component parentComp)
            {
                if (parentComp)
                {
                    var gObj = parentComp.gameObject;
                    fileName = $"{gObj.name}_{elementType.Name}.asset";
                }
                else
                {
                    fileName = $"0_{elementType.Name}_{Property.Name}.asset";
                }
            }
            else
            {
                fileName = $"New_{elementType.Name}.asset";
            }

            // 使用路徑配置建立新的 ScriptableObject
            var newSO = CreateScriptableObjectWithSelectedPath(elementType, fileName);

            // 將新建立的 ScriptableObject 加入到 List 中
            if (newSO != null)
            {
                var list = Property.ValueEntry.WeakSmartValue as IList;
                if (list == null)
                {
                    // 如果 List 為 null，建立新的 List
                    list = (IList)Activator.CreateInstance(listType);
                    Property.ValueEntry.WeakSmartValue = list;
                }

                list.Add(newSO);
                Property.ValueEntry.ApplyChanges();
            }
        }

        /// <summary>
        /// 繪製路徑選擇 GUI
        /// </summary>
        private void DrawPathSelector(Type configType)
        {
            var config = SOPathSettingConfig.Instance;
            var basePath = config.GetBasePathForType(configType);
            // Debug.Log("Current Base Path For Type: " + configType.Name + " is " + basePath);
            var availablePaths = GetAvailablePaths();
            
            EditorGUILayout.BeginHorizontal();
            
            // 路徑選擇下拉選單
            var selectedIndex = availablePaths.IndexOf(basePath);
            if (selectedIndex < 0)
            {
                selectedIndex = 0; // 預設選擇 Assets
            }
            
            EditorGUILayout.LabelField("建立路徑:", GUILayout.Width(60));
            var newIndex = EditorGUILayout.Popup(selectedIndex, availablePaths.ToArray());
            
            if (newIndex != selectedIndex && newIndex >= 0)
            {
                // 路徑變更時更新配置
                Debug.Log("Set Path For Type: Attribute.SubFolderPath " +  Attribute.SubFolderPath);
                var newPath = availablePaths[newIndex];
                config.SetPathForType(configType, newPath,subPath: Attribute.SubFolderPath);
                
                EditorUtility.SetDirty(config);
            }
            
            EditorGUILayout.EndHorizontal();
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            // EditorGUILayout.BeginFadeGroup(1);
            // 檢查是否為 List 類型
            SirenixEditorGUI.BeginBox();
            var valueType = Property.ValueEntry.TypeOfValue;
            var isListType = typeof(IList).IsAssignableFrom(valueType);
            
            if (isListType)
            {
                // 對於 List 類型，繪製預設內容和 Create 按鈕
                CallNextDrawer(label);
                
                // 取得 List 的元素類型來顯示路徑選擇器
                var elementType = valueType.GetGenericArguments()[0];
                DrawPathSelector(elementType);
                
                DrawCreateButtonForList();
                SirenixEditorGUI.EndBox();
                return;
            }
            
            // 原有的單一物件檢查
            if ((UnityEngine.Object)Property.ValueEntry.WeakSmartValue != null)
            {
                CallNextDrawer(label);
                SirenixEditorGUI.EndBox();
                return;
            }

            CallNextDrawer(label);
            
            // 顯示路徑選擇器
            var configType = Property.ValueEntry.TypeOfValue;
            DrawPathSelector(configType);
            
            // 建立按鈕
            var guiContent = new GUIContent("Create", null, "Create a new ScriptableObject for this field");

            var buttonClicked = SirenixEditorGUI.SDFIconButton(
                EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight),
                guiContent,
                SdfIconType.FileEarmarkSpreadsheet,
                IconAlignment.LeftEdge);

            if (buttonClicked)
            {
                if (Property.ParentValues[0] is ScriptableObject)
                {
                    CreateSOForSO();
                }
                else if (Property.ParentValues[0] is Component)
                {
                    CreateSOForMonoBehavior();
                }
            }
            SirenixEditorGUI.EndBox();
            // EditorGUILayout.EndFadeGroup();
        }
    }
}
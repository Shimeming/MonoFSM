// using _1_MonoFSM_Core.Runtime.LifeCycle.Update;
// using Sirenix.OdinInspector;
// using Sirenix.OdinInspector.Editor;
// using Sirenix.Utilities.Editor;
// using UnityEditor;
// using UnityEngine;
//
// namespace MonoFSM.Core.Editor
// {
//     /// <summary>
//     /// MonoModuleFolder 的自定義 Drawer
//     /// 提供一鍵添加所有 Prefab 到 GameObject 下的功能
//     /// </summary>
//     public class MonoModuleFolderDrawer : OdinValueDrawer<MonoModuleFolder>
//     {
//         protected override void DrawPropertyLayout(GUIContent label)
//         {
//             var moduleFolder = ValueEntry.SmartValue;
//
//             // 繪製默認的 Inspector
//             CallNextDrawer(label);
//
//             // 如果沒有選擇任何 Prefab，則不顯示按鈕
//             if (moduleFolder.PrefabModules == null || moduleFolder.PrefabModules.Length == 0)
//             {
//                 SirenixEditorGUI.InfoMessageBox("請先選擇要添加的 Prefab 模組");
//                 return;
//             }
//
//             // 計算有效的 Prefab 數量
//             int validPrefabCount = 0;
//             foreach (var prefab in moduleFolder.PrefabModules)
//             {
//                 if (prefab != null)
//                     validPrefabCount++;
//             }
//
//             if (validPrefabCount == 0)
//             {
//                 SirenixEditorGUI.WarningMessageBox("所選的 Prefab 模組列表中沒有有效的 Prefab");
//                 return;
//             }
//
//             GUILayout.Space(5);
//
//             // 顯示子物件資訊
//             int currentChildCount = moduleFolder.transform.childCount;
//             if (currentChildCount > 0)
//             {
//                 SirenixEditorGUI.InfoMessageBox($"當前子物件數量: {currentChildCount}");
//             }
//
//             // 繪製操作按鈕區域
//             using (new GUILayout.HorizontalScope())
//             {
//                 GUILayout.FlexibleSpace();
//
//                 // 添加所有 Prefab 按鈕
//                 GUI.backgroundColor = new Color(0.6f, 1f, 0.6f);
//                 if (SirenixEditorGUI.SDFIconButton(
//                         $"添加所有模組 ({validPrefabCount})",
//                         24,
//                         SdfIconType.PlusCircleFill,
//                         IconAlignment.LeftOfText))
//                 {
//                     AddAllPrefabsAsChildren(moduleFolder);
//                 }
//
//                 GUI.backgroundColor = Color.white;
//
//                 GUILayout.Space(10);
//
//                 // 清除所有子物件按鈕 (只有當有子物件時才顯示)
//                 if (currentChildCount > 0)
//                 {
//                     GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
//                     if (SirenixEditorGUI.SDFIconButton(
//                             $"清除子物件 ({currentChildCount})",
//                             24,
//                             SdfIconType.TrashFill,
//                             IconAlignment.LeftOfText))
//                     {
//                         if (EditorUtility.DisplayDialog(
//                                 "確認清除",
//                                 $"確定要刪除 {currentChildCount} 個子物件嗎？此操作可以使用 Undo (Ctrl+Z) 復原。",
//                                 "確定",
//                                 "取消"))
//                         {
//                             ClearAllChildren(moduleFolder);
//                         }
//                     }
//
//                     GUI.backgroundColor = Color.white;
//                 }
//
//                 GUILayout.FlexibleSpace();
//             }
//
//             GUILayout.Space(5);
//         }
//
//         /// <summary>
//         /// 添加所有 Prefab 作為子物件
//         /// </summary>
//         private void AddAllPrefabsAsChildren(MonoModuleFolder moduleFolder)
//         {
//             if (moduleFolder.PrefabModules == null || moduleFolder.PrefabModules.Length == 0)
//                 return;
//
//             Undo.RegisterCompleteObjectUndo(moduleFolder.gameObject, "Add All Module Prefabs");
//
//             int addedCount = 0;
//             foreach (var prefabModule in moduleFolder.PrefabModules)
//             {
//                 if (prefabModule == null)
//                     continue;
//
//                 // 在編輯器模式下實例化 Prefab
//                 GameObject instance =
//                     PrefabUtility.InstantiatePrefab(prefabModule.gameObject) as GameObject;
//
//                 if (instance != null)
//                 {
//                     // 設置為 moduleFolder 的子物件
//                     instance.transform.SetParent(moduleFolder.transform, false);
//
//                     // 重置 Transform
//                     instance.transform.localPosition = Vector3.zero;
//                     instance.transform.localRotation = Quaternion.identity;
//                     instance.transform.localScale = Vector3.one;
//
//                     // 註冊 Undo
//                     Undo.RegisterCreatedObjectUndo(instance, "Add Module Prefab");
//                     addedCount++;
//                 }
//             }
//
//             // 標記場景為已修改
//             if (addedCount > 0)
//             {
//                 EditorUtility.SetDirty(moduleFolder.gameObject);
//                 Debug.Log(
//                     $"[MonoModuleFolder] 成功添加 {addedCount} 個模組到 {moduleFolder.gameObject.name}");
//             }
//             else
//             {
//                 Debug.LogWarning($"[MonoModuleFolder] 沒有有效的 Prefab 可以添加");
//             }
//         }
//
//         /// <summary>
//         /// 清除所有子物件
//         /// </summary>
//         private void ClearAllChildren(MonoModuleFolder moduleFolder)
//         {
//             int childCount = moduleFolder.transform.childCount;
//
//             if (childCount == 0)
//             {
//                 Debug.Log($"[MonoModuleFolder] {moduleFolder.gameObject.name} 沒有子物件");
//                 return;
//             }
//
//             Undo.RegisterCompleteObjectUndo(moduleFolder.gameObject, "Clear All Children");
//
//             // 從後往前刪除，避免索引問題
//             for (int i = childCount - 1; i >= 0; i--)
//             {
//                 Transform child = moduleFolder.transform.GetChild(i);
//                 Undo.DestroyObjectImmediate(child.gameObject);
//             }
//
//             EditorUtility.SetDirty(moduleFolder.gameObject);
//             Debug.Log($"[MonoModuleFolder] 已清除 {moduleFolder.gameObject.name} 的 {childCount} 個子物件");
//         }
//     }
// }



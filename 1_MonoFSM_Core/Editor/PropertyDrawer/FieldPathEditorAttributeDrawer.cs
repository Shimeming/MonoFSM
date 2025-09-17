using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MonoDebugSetting;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.DataProvider;
using MonoFSM.Core.Utilities;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace MonoFSM.Core.Editor
{
    /// <summary>
    ///     為標記了 FieldPathEditorAttribute 的 List&lt;FieldPathEntry&gt; 欄位提供路徑編輯介面
    /// </summary>
    [DrawerPriority(2)]
    public class FieldPathEditorAttributeDrawer
        : OdinAttributeDrawer<FieldPathEditorAttribute, List<FieldPathEntry>>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (RuntimeDebugSetting.IsDebugMode)
            {
                CallNextDrawer(label);
                return;
            }
            // CallNextDrawer(label);
            var pathEntries = ValueEntry.SmartValue ?? new List<FieldPathEntry>();
            var rootType = GetRootType();
            // Debug.Log("Root Type: " + (rootType != null ? rootType.FullName : "null"));
            if (rootType == null)
            {
                SirenixEditorGUI.ErrorMessageBox(Attribute.NoRootTypeMessage);
                return;
            }

            SirenixEditorGUI.BeginBox();

            // 顯示標題
            if (label != null)
                EditorGUILayout.LabelField(label.text, EditorStyles.boldLabel);

            EditorGUILayout.Space(5);

            // 繪製路徑編輯器
            var newEntries = DrawPathSegments(pathEntries, rootType);

            // 如果路徑有變化，更新
            if (!ArePathEntriesEqual(pathEntries, newEntries))
            {
                ValueEntry.SmartValue = newEntries;
                // 觸發 OnPathEntriesChanged（如果目標物件有此方法）
                TriggerOnPathEntriesChanged();
                Debug.Log("Field path updated.");
            }

            EditorGUILayout.Space(5);

            // 添加操作按鈕
            DrawPathOperationButtons(pathEntries);

            SirenixEditorGUI.EndBox();
        }

        /// <summary>
        ///     獲取根型別
        /// </summary>
        private Type GetRootType()
        {
            var targetObject = Property.ParentValues[0];
            if (targetObject == null)
                return null;

            // 優先級1: 使用動態 IFieldPathRootTypeProvider 介面
            if (Attribute.UseDynamicRootType && targetObject is IFieldPathRootTypeProvider provider)
                try
                {
                    var fieldName = Property.Name;
                    var rootType = provider.GetFieldPathRootType();
                    if (rootType != null)
                        return rootType;
                }
                catch (Exception e)
                {
                    Debug.LogError(
                        $"透過 IFieldPathRootTypeProvider 獲取根型別時發生錯誤: {e.Message}"
                    );
                }

            // 優先級2: 使用 RootTypeProvider 方法
            if (!string.IsNullOrEmpty(Attribute.RootTypeProvider))
            {
                var method = targetObject
                    .GetType()
                    .GetMethod(
                        Attribute.RootTypeProvider,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                    );

                if (method != null && method.ReturnType == typeof(Type))
                    try
                    {
                        return method.Invoke(targetObject, null) as Type;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(
                            $"調用 {Attribute.RootTypeProvider} 方法時發生錯誤: {e.Message}"
                        );
                    }

                // 如果是屬性
                var property = targetObject
                    .GetType()
                    .GetProperty(
                        Attribute.RootTypeProvider,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                    );

                if (property != null && property.PropertyType == typeof(Type))
                    try
                    {
                        return property.GetValue(targetObject) as Type;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(
                            $"讀取 {Attribute.RootTypeProvider} 屬性時發生錯誤: {e.Message}"
                        );
                    }
            }

            // 優先級3: 使用 RootTypeName
            if (!string.IsNullOrEmpty(Attribute.RootTypeName))
                return Type.GetType(Attribute.RootTypeName)
                    ?? AppDomain
                        .CurrentDomain.GetAssemblies()
                        .SelectMany(a => a.GetTypes())
                        .FirstOrDefault(t => t.Name == Attribute.RootTypeName);

            return null;
        }

        /// <summary>
        ///     觸發 OnPathEntriesChanged 方法（如果存在）
        /// </summary>
        private void TriggerOnPathEntriesChanged()
        {
            var targetObject = Property.ParentValues[0];
            if (targetObject != null)
            {
                var method = targetObject
                    .GetType()
                    .GetMethod(
                        "OnPathEntriesChanged",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                    );
                method?.Invoke(targetObject, null);
            }
        }

        /// <summary>
        ///     繪製路徑片段
        /// FIXME: 勾選的內容沒有正確觸發 prefab override的標記，editor code要怎麼顯示才會正確？
        /// </summary>
        private List<FieldPathEntry> DrawPathSegments(
            List<FieldPathEntry> currentEntries,
            Type rootType
        )
        {
            var currentType = rootType;
            const int maxSegments = 6; // 最多6個層級

            for (var i = 0; i < maxSegments && i < currentEntries.Count; i++)
            {
                if (currentType == null)
                    break;

                var currentSegment = currentEntries[i];

                // 繪製屬性選擇器
                EditorGUILayout.BeginHorizontal();

                // 添加CanBeNull checkbox
                EditorGUI.BeginChangeCheck();
                var canBeNullContent = new GUIContent(
                    "",
                    "勾選時允許此層級為null值，不會拋出NullReference異常"
                );
                var newCanBeNull = EditorGUILayout.Toggle(
                    canBeNullContent,
                    currentSegment._canBeNull,
                    GUILayout.Width(20)
                );
                if (EditorGUI.EndChangeCheck())
                {
                    currentSegment._canBeNull = newCanBeNull;
                    ValueEntry.SmartValue = currentEntries; // 觸發更新
                }

                EditorGUILayout.LabelField($"層級 {i + 1}:", GUILayout.Width(50));

                // 使用OdinSelector來選擇屬性
                var currentPropertyName = currentSegment._propertyName ?? "";

                var displayText = string.IsNullOrEmpty(currentPropertyName)
                    ? "-- 選擇屬性 --"
                    : currentPropertyName;

                // 檢查屬性是否存在
                var isPropertyValid =
                    string.IsNullOrEmpty(currentPropertyName)
                    || (
                        GetMemberType(currentType, currentPropertyName) != null
                        && GetAvailableMembersForEntry(currentType, currentSegment)
                            .Contains(currentPropertyName)
                    );

                var buttonStyle = isPropertyValid
                    ? EditorStyles.popup
                    : new GUIStyle(EditorStyles.popup) { normal = { textColor = Color.red } };

                if (!isPropertyValid)
                    displayText = $"⚠ {currentPropertyName} (不存在)";

                var selectorRect = EditorGUILayout.GetControlRect();

                if (GUI.Button(selectorRect, displayText, buttonStyle))
                    ShowPropertySelector(currentType, currentPropertyName, i, currentEntries);

                // 檢查是否為陣列
                if (!string.IsNullOrEmpty(currentPropertyName) && isPropertyValid)
                {
                    var memberType = GetMemberType(currentType, currentPropertyName);
                    if (memberType != null && memberType.IsArray)
                    {
                        // 顯示陣列索引輸入
                        EditorGUILayout.LabelField("[", GUILayout.Width(10));

                        EditorGUI.BeginChangeCheck();
                        var newIndex = EditorGUILayout.IntField(
                            currentSegment.index,
                            GUILayout.Width(30)
                        );
                        if (EditorGUI.EndChangeCheck())
                        {
                            currentSegment.index = Math.Max(0, newIndex);
                            ValueEntry.SmartValue = currentEntries; // 觸發更新
                        }

                        EditorGUILayout.LabelField("]", GUILayout.Width(10));
                        currentType = memberType.GetElementType();
                    }
                    else
                    {
                        currentType = memberType;
                    }
                }
                else if (!isPropertyValid)
                {
                    // 如果屬性無效，停止處理後續類型鏈
                    currentType = null;
                }
                else
                {
                    EditorGUILayout.EndHorizontal();
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            // 顯示添加新層級的按鈕
            if (currentEntries.Count < maxSegments && currentType != null)
            {
                // 創建一個臨時的 FieldPathEntry 來檢查可用成員
                var tempEntry = new FieldPathEntry();
                tempEntry.SetSerializedType(currentType);
                // 如果有現有的entries，複製其_supportedTypes設定
                if (currentEntries.Count > 0)
                    tempEntry._supportedTypes = currentEntries[0]._supportedTypes;

                var availableMembers = GetAvailableMembersForEntry(currentType, tempEntry);
                if (availableMembers.Count > 0)
                {
                    EditorGUILayout.BeginHorizontal();

                    // 新層級的CanBeNull checkbox（預設為false）
                    var newLevelCanBeNullContent = new GUIContent(
                        "",
                        "新層級的CanBeNull設定（預設為false）"
                    );
                    EditorGUILayout.Toggle(newLevelCanBeNullContent, false, GUILayout.Width(20));
                    EditorGUI.BeginDisabledGroup(true); // 禁用，因為還沒有實際的entry
                    EditorGUILayout.LabelField(
                        $"層級 {currentEntries.Count + 1}:",
                        GUILayout.Width(50)
                    );
                    EditorGUI.EndDisabledGroup();

                    var addButtonRect = EditorGUILayout.GetControlRect();
                    if (GUI.Button(addButtonRect, "+ 添加屬性", EditorStyles.popup))
                        ShowPropertySelector(currentType, "", currentEntries.Count, currentEntries);

                    EditorGUILayout.EndHorizontal();
                }
            }

            return currentEntries; // 返回原始entries，修改會直接反映在原始列表中
        }

        /// <summary>
        ///     繪製路徑操作按鈕
        /// </summary>
        private void DrawPathOperationButtons(List<FieldPathEntry> pathEntries)
        {
            if (pathEntries.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();

                // 移除最後一層級按鈕
                if (GUILayout.Button("⬅ 移除最後一層", EditorStyles.miniButton))
                {
                    var modifiedEntries = new List<FieldPathEntry>(pathEntries);
                    modifiedEntries.RemoveAt(modifiedEntries.Count - 1);
                    ValueEntry.SmartValue = modifiedEntries;
                    TriggerOnPathEntriesChanged();
                }

                // 清空全部按鈕
                var clearButtonStyle = new GUIStyle(EditorStyles.miniButton)
                {
                    normal = { textColor = Color.red },
                };
                if (GUILayout.Button("✕ 清空全部", clearButtonStyle))
                {
                    ValueEntry.SmartValue = new List<FieldPathEntry>();
                    TriggerOnPathEntriesChanged();
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        ///     顯示屬性選擇器
        /// </summary>
        private void ShowPropertySelector(
            Type currentType,
            string currentPropertyName,
            int levelIndex,
            List<FieldPathEntry> currentEntries
        )
        {
            var selector = new FieldPathPropertySelector(currentType, currentPropertyName);
            var rect = GUILayoutUtility.GetLastRect();
            selector.ShowInPopup(rect, 400);

            selector.SelectionConfirmed += selection =>
            {
                var selectedProperty = selection.FirstOrDefault();
                HandlePropertySelection(selectedProperty, levelIndex, currentEntries, currentType);
            };
        }

        /// <summary>
        ///     處理屬性選擇
        /// </summary>
        private void HandlePropertySelection(
            string selectedProperty,
            int levelIndex,
            List<FieldPathEntry> currentEntries,
            Type currentType
        )
        {
            if (string.IsNullOrEmpty(selectedProperty))
                return;

            // 如果是修改現有層級
            if (levelIndex < currentEntries.Count)
            {
                // 修改當前層級
                currentEntries[levelIndex]._propertyName = selectedProperty;
                currentEntries[levelIndex].SetSerializedType(currentType);

                // 移除後續層級（因為型別可能改變了）
                while (currentEntries.Count > levelIndex + 1)
                    currentEntries.RemoveAt(currentEntries.Count - 1);
            }
            else
            {
                // 添加新層級
                var newEntry = new FieldPathEntry();
                newEntry._propertyName = selectedProperty;
                newEntry.SetSerializedType(currentType);
                currentEntries.Add(newEntry);
            }

            // 觸發更新
            ValueEntry.SmartValue = currentEntries;
            TriggerOnPathEntriesChanged();
        }

        /// <summary>
        ///     比較兩個路徑列表是否相等
        /// </summary>
        private bool ArePathEntriesEqual(List<FieldPathEntry> a, List<FieldPathEntry> b)
        {
            if (a == null && b == null)
                return true;
            if (a == null || b == null)
                return false;
            if (a.Count != b.Count)
                return false;

            for (var i = 0; i < a.Count; i++)
                if (a[i]._propertyName != b[i]._propertyName || a[i].index != b[i].index)
                    return false;

            return true;
        }

        /// <summary>
        ///     獲取型別的可用成員（使用特定FieldPathEntry的過濾邏輯）
        /// </summary>
        private List<string> GetAvailableMembersForEntry(Type type, FieldPathEntry entry)
        {
            if (type == null || entry == null)
                return new List<string>();

            // 設定entry的父型別
            entry.SetSerializedType(type);

            // 使用entry的方法：包含欄位，只包含公共成員（與原本邏輯保持一致）
            return entry.GetAvailableMembers(type, true);
        }

        /// <summary>
        ///     獲取成員的型別（使用統一的 ReflectionUtility）
        /// </summary>
        private Type GetMemberType(Type type, string memberName)
        {
            if (type == null || string.IsNullOrEmpty(memberName))
                return null;

            // 使用 ReflectionUtility 的快取機制來提高效率，與 FieldPathEntry 保持一致
            return ReflectionUtility.GetMemberType(type, memberName);
        }
    }
}

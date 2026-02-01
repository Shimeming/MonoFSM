#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace CommandPalette
{
    /// <summary>
    /// EditorWindow 搜尋輔助工具
    /// </summary>
    public static class EditorWindowSearchHelper
    {
        private static List<EditorWindowEntry> _cachedWindowEntries;

        /// <summary>
        /// 取得所有 EditorWindow 類型
        /// </summary>
        public static List<EditorWindowEntry> GetAllEditorWindowTypes()
        {
            if (_cachedWindowEntries != null)
                return _cachedWindowEntries;

            _cachedWindowEntries = new List<EditorWindowEntry>();

            var allWindowTypes = TypeCache.GetTypesDerivedFrom<EditorWindow>();

            foreach (var type in allWindowTypes)
            {
                // 過濾抽象類別
                if (type.IsAbstract)
                    continue;

                // 過濾非公開類別
                if (!type.IsPublic && !type.IsNestedPublic)
                    continue;

                // 過濾泛型類別
                if (type.IsGenericType)
                    continue;

                var displayName = GetDisplayName(type);
                var category = GetCategory(type);

                _cachedWindowEntries.Add(new EditorWindowEntry(type, displayName, category));
            }

            // 按類別和名稱排序
            _cachedWindowEntries = _cachedWindowEntries
                .OrderBy(e => e.Category)
                .ThenBy(e => e.DisplayName)
                .ToList();

            return _cachedWindowEntries;
        }

        /// <summary>
        /// 取得視窗的顯示名稱
        /// </summary>
        private static string GetDisplayName(Type type)
        {
            // 嘗試從 AddComponentMenu 或其他特性取得顯示名稱
            var titleAttribute = type.GetCustomAttribute<System.ComponentModel.DisplayNameAttribute>();
            if (titleAttribute != null)
                return titleAttribute.DisplayName;

            // 將類別名稱轉換為易讀格式（例如 "SceneView" -> "Scene View"）
            return FormatTypeName(type.Name);
        }

        /// <summary>
        /// 將 PascalCase 轉換為空格分隔的格式
        /// </summary>
        private static string FormatTypeName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return typeName;

            // 移除常見的後綴
            if (typeName.EndsWith("Window"))
                typeName = typeName.Substring(0, typeName.Length - 6);
            else if (typeName.EndsWith("EditorWindow"))
                typeName = typeName.Substring(0, typeName.Length - 12);

            // 在大寫字母前插入空格
            var result = "";
            for (int i = 0; i < typeName.Length; i++)
            {
                if (i > 0 && char.IsUpper(typeName[i]) && !char.IsUpper(typeName[i - 1]))
                    result += " ";
                result += typeName[i];
            }

            return result.Trim();
        }

        /// <summary>
        /// 根據類別的命名空間判斷類別
        /// </summary>
        private static string GetCategory(Type type)
        {
            var ns = type.Namespace ?? "";

            if (ns.StartsWith("UnityEditor") || ns.StartsWith("Unity."))
                return "Unity";

            if (ns.Contains("MonoFSM") || ns.Contains("CommandPalette"))
                return "MonoFSM";

            // 檢查是否為常見的第三方套件
            if (ns.Contains("Odin") || ns.Contains("Sirenix"))
                return "Odin";

            if (string.IsNullOrEmpty(ns))
                return "Project";

            return "Third Party";
        }

        /// <summary>
        /// 開啟指定的 EditorWindow
        /// </summary>
        public static void OpenEditorWindow(EditorWindowEntry entry)
        {
            if (entry?.Type == null)
                return;

            try
            {
                // 使用 EditorWindow.GetWindow 開啟視窗
                var getWindowMethod = typeof(EditorWindow).GetMethod(
                    "GetWindow",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new[] { typeof(Type), typeof(bool) },
                    null
                );

                if (getWindowMethod != null)
                {
                    var window = getWindowMethod.Invoke(null, new object[] { entry.Type, false }) as EditorWindow;
                    window?.Focus();
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[CommandPalette] 無法開啟視窗 {entry.DisplayName}: {e.Message}");
            }
        }

        /// <summary>
        /// 清除快取（當需要重新掃描時）
        /// </summary>
        public static void ClearCache()
        {
            _cachedWindowEntries = null;
        }
    }
}
#endif

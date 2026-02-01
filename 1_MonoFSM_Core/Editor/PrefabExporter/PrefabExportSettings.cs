using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace MonoFSM.Editor
{
    /// <summary>
    /// Prefab 匯出設定
    /// </summary>
    [Serializable]
    public class PrefabExportSettings
    {
        private const string EditorPrefsKey = "MonoFSM_PrefabExportSettings";

        [Title("Component 篩選")]
        [LabelText("只包含指定的 Component 類型")]
        [InfoBox("留空則匯出所有 Component")]
        [ValueDropdown("GetAllComponentTypes", IsUniqueList = true, DrawDropdownForListElements = false)]
        public List<string> _includedComponentTypeNames = new();

        [Title("欄位篩選")]
        [LabelText("排除預設值")]
        [Tooltip("只輸出與預設值不同的欄位")]
        public bool _excludeDefaults = true;

        [LabelText("只顯示 Public 欄位")]
        [Tooltip("排除 [SerializeField] private 欄位")]
        public bool _onlyPublicFields = false;

        [LabelText("排除的欄位名稱")]
        [Tooltip("這些欄位名稱將被排除")]
        public List<string> _excludedFieldNames = new()
        {
            "m_Script",
            "m_ObjectHideFlags"
        };

        [Title("Transform 設定")]
        [LabelText("排除預設 Transform")]
        [Tooltip("如果 Transform 是預設值 (position=0, rotation=0, scale=1) 則不輸出")]
        public bool _excludeDefaultTransform = true;

        [LabelText("使用 Local 座標")]
        [Tooltip("使用相對於父物件的座標")]
        public bool _useLocalCoordinates = true;

        [Title("輸出格式")]
        [LabelText("包含註解")]
        [Tooltip("在 Component 前加上註解")]
        public bool _includeComments = true;

        [LabelText("縮排字元")]
        public string _indentString = "  ";

        // 快取的 Type 列表
        [NonSerialized]
        private HashSet<Type> _includedTypes;

        public HashSet<Type> IncludedComponentTypes
        {
            get
            {
                if (_includedTypes == null)
                {
                    _includedTypes = new HashSet<Type>();
                    foreach (var typeName in _includedComponentTypeNames)
                    {
                        var type = Type.GetType(typeName) ??
                                   AppDomain.CurrentDomain.GetAssemblies()
                                       .SelectMany(a => a.GetTypes())
                                       .FirstOrDefault(t => t.FullName == typeName || t.Name == typeName);
                        if (type != null)
                            _includedTypes.Add(type);
                    }
                }
                return _includedTypes;
            }
        }

        public bool ShouldIncludeComponent(Type componentType)
        {
            // 如果沒有指定任何類型，則包含所有
            if (_includedComponentTypeNames.Count == 0)
                return true;

            return IncludedComponentTypes.Contains(componentType) ||
                   IncludedComponentTypes.Any(t => t.IsAssignableFrom(componentType));
        }

        public bool ShouldExcludeField(string fieldName)
        {
            return _excludedFieldNames.Contains(fieldName);
        }

        public void InvalidateTypeCache()
        {
            _includedTypes = null;
        }

        public void Save()
        {
            var json = JsonUtility.ToJson(this);
            EditorPrefs.SetString(EditorPrefsKey, json);
        }

        public static PrefabExportSettings Load()
        {
            var json = EditorPrefs.GetString(EditorPrefsKey, string.Empty);
            if (string.IsNullOrEmpty(json))
                return new PrefabExportSettings();

            try
            {
                return JsonUtility.FromJson<PrefabExportSettings>(json);
            }
            catch
            {
                return new PrefabExportSettings();
            }
        }

        public static PrefabExportSettings CreateDefault()
        {
            return new PrefabExportSettings();
        }

        public static PrefabExportSettings CreateQuickCopy()
        {
            return new PrefabExportSettings
            {
                _excludeDefaults = true,
                _onlyPublicFields = false,
                _excludeDefaultTransform = true,
                _useLocalCoordinates = true,
                _includeComments = true
            };
        }

        private IEnumerable<string> GetAllComponentTypes()
        {
            var componentTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                })
                .Where(t => typeof(Component).IsAssignableFrom(t) &&
                            !t.IsAbstract &&
                            t != typeof(Transform) &&
                            !t.FullName.StartsWith("UnityEngine.Internal"))
                .OrderBy(t => t.Namespace)
                .ThenBy(t => t.Name)
                .Select(t => t.FullName);

            return componentTypes;
        }
    }
}

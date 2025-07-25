using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.Utilities;
using MonoFSM.Variable;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace MonoFSM.Core.DataProvider
{
    /// <summary>
    ///     表示欄位路徑上單一層級的資料結構
    /// </summary>
    [Serializable]
    public class FieldPathEntry
    {
        public void SetSerializedType(Type type)
        {
            _serializedType = new MySerializedType<Object>
            {
                RestrictType = type
            };
        }

        // 父層型別，由外部更新（非序列化）
        // [NonSerialized] public Type parentType;
        [InlineProperty(LabelWidth = 60)] public MySerializedType<Object> _serializedType; //FIXME: refactor時會爛掉...有點麻煩

#if UNITY_EDITOR
        [NonSerialized] [PreviewInDebugMode] public object _tempCurrentObject;
#endif
        
        public Type GetPropertyType()
        {
            if (string.IsNullOrEmpty(_propertyName)) return null;
            
            // 使用 ReflectionUtility 的快取機制來提高效率
            var memberType = ReflectionUtility.GetMemberType(parentType, _propertyName);

            if (IsArray && memberType is { IsArray: true })
                return memberType.GetElementType();

            return memberType;
        }

        [FormerlySerializedAs("fieldName")] [ValueDropdown(nameof(GetFieldOptions))]
        public string _propertyName;

        public string PropertyPath
        {
            get
            {
                if (string.IsNullOrEmpty(_propertyName))
                    return string.Empty;

                // 如果是陣列，則加上索引
                if (IsArray)
                    return $"{_propertyName}[{index}]";

                // 否則只返回欄位名稱
                return _propertyName;
            }
        }

        // 當對應的欄位為陣列時，才會顯示 index 欄位
        //FIXME: 不可以編輯？用index注入？
        // [PreviewInInspector]
        [ShowIf(nameof(IsArray))] [LabelText("Index")]
        public int index; //injected index;

        private Type parentType => _serializedType.RestrictType; //為什麼叫parentType？
        

        // 支援的型別清單
        //restrict to types?
        //FIXME: editor only?
        [ShowInDebugMode]
        [PreviewInInspector] public List<Type> _supportedTypes;

        /// <summary>
        ///     統一的成員獲取方法，可用於不同場景
        /// </summary>
        public List<string> GetAvailableMembers(Type targetType = null, bool includeFields = false, bool includeNonPublic = false)
        {
            var type = targetType ?? parentType;
            if (type == null) return new List<string>();

            var members = new List<string>();
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public;
            if (includeNonPublic)
                bindingFlags |= BindingFlags.NonPublic;

            // 獲取屬性
            var properties = type.GetProperties(bindingFlags)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0); // 排除索引器

            foreach (var prop in properties)
            {
                if (IsValidMember(prop.PropertyType))
                    members.Add(prop.Name);
            }

            // 獲取欄位（如果需要）
            if (includeFields)
            {
                var fields = type.GetFields(bindingFlags);
                foreach (var field in fields)
                {
                    if (IsValidMember(field.FieldType))
                        members.Add(field.Name);
                }
            }

            return members.Distinct().OrderBy(m => m).ToList();
        }

        /// <summary>
        ///     檢查成員型別是否有效
        /// </summary>
        private bool IsValidMember(Type memberType)
        {
            // 如果沒有設定 _supportedTypes，則允許所有型別（包括陣列）
            if (_supportedTypes == null || _supportedTypes.Count == 0)
                return true; // 允許所有型別

            // 如果有設定 _supportedTypes，則檢查是否為支援的型別或陣列
            return memberType.IsArray || _supportedTypes.Contains(memberType);
        }

        /// <summary>
        ///     動態回傳 parentType 中所有可存取的欄位與屬性名稱
        /// </summary>
        public IEnumerable<ValueDropdownItem<string>> GetFieldOptions()
        {
            var options = new List<ValueDropdownItem<string>>();
            var pType = _serializedType.RestrictType;
            
            if (pType != null)
            {
                // 使用統一的成員獲取方法，但保持原有的非公共屬性支援
                var properties = pType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                
                foreach (var prop in properties)
                {
                    if (!prop.CanRead) continue;

                    var propType = prop.PropertyType;
                    if (IsValidMember(propType))
                    {
                        options.Add(new ValueDropdownItem<string>($"{prop.Name}:{propType}", prop.Name));
                    }
                }
            }

            return options;
        }

        /// <summary>
        ///     判斷 parentType 中選擇的欄位是否為陣列
        /// </summary>
        public bool IsArray
        {
            get
            {
                if (parentType == null || string.IsNullOrEmpty(_propertyName))
                    return false;

                // 使用 ReflectionUtility 的快取機制來提高效率
                var memberType = ReflectionUtility.GetMemberType(parentType, _propertyName);
                return memberType?.IsArray ?? false;
            }
        }

        public bool _canBeNull;
    }
}
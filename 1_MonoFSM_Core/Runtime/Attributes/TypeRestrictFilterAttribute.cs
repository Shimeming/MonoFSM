using System;
using MonoFSM.Runtime.Mono;
using MonoFSM.Variable;
using UnityEngine;

namespace _1_MonoFSM_Core.Runtime.Attributes
{
    /// <summary>
    /// 用於過濾帶有 RestrictType 屬性的欄位的通用 Attribute
    /// 可以指定期望的限制型別來過濾下拉選單中的選項
    /// 支援 VariableTag 和 MonoEntityTag 等類型，以及任何 ScriptableObject
    /// </summary>
    public class TypeRestrictFilterAttribute : Attribute
    {
        /// <summary>
        /// 期望的限制型別 (從 ScriptableObject 實例中取得的型別，如 VariableTag.VariableMonoType)
        /// </summary>
        public Type RestrictInstanceType { get; }
        
        /// <summary>
        /// 是否允許相容的類型
        /// </summary>
        public bool AllowCompatibleTypes { get; }
        
        /// <summary>
        /// 自定義錯誤訊息
        /// </summary>
        public string CustomErrorMessage { get; }

        /// <summary>
        /// 無參數構造函數 - 將不限制 RestrictType，接受任何 Property Type 的實例
        /// </summary>
        public TypeRestrictFilterAttribute() : this(null)
        {
        }
        
        /// <summary>
        /// 構造函數
        /// </summary>
        /// <param name="restrictInstanceType">期望的限制型別</param>
        /// <param name="allowCompatibleTypes">是否允許相容的類型</param>
        /// <param name="customErrorMessage">自定義錯誤訊息</param>
        public TypeRestrictFilterAttribute(Type restrictInstanceType, bool allowCompatibleTypes = true,
            string customErrorMessage = null)
        {
            RestrictInstanceType = restrictInstanceType;
            AllowCompatibleTypes = allowCompatibleTypes;
            CustomErrorMessage = customErrorMessage;
        }
        
        /// <summary>
        /// 獲取目標物件的 RestrictType
        /// </summary>
        public Type GetRestrictType(object target)
        {
            return target switch
            {
                VariableTag variableTag => variableTag.VariableMonoType,
                MonoEntityTag entityTag => entityTag.RestrictType,
                ScriptableObject scriptableObject => scriptableObject.GetType(),
                _ => target?.GetType()
            };
        }
        
        /// <summary>
        /// 獲取 Asset 搜尋過濾器（基於 Property Type）
        /// </summary>
        public string GetAssetSearchFilter(Type propertyType)
        {
            if (propertyType == typeof(VariableTag))
                return "t:VariableTag";
            if (propertyType == typeof(MonoEntityTag))
                return "t:MonoEntityTag";

            // 通用處理：對於任何 ScriptableObject，搜尋所有 ScriptableObject
            if (typeof(ScriptableObject).IsAssignableFrom(propertyType))
                return "t:ScriptableObject";
            
            // 通用處理：嘗試從類型名稱推斷
            return $"t:{propertyType.Name}";
        }
    }
    
    /// <summary>
    /// VarTagFilter 的向後相容別名
    /// </summary>
    public class VarTagFilterAttribute : TypeRestrictFilterAttribute
    {
        public VarTagFilterAttribute(Type expectedVariableType, bool allowCompatibleTypes = true, string customErrorMessage = null)
            : base(expectedVariableType, allowCompatibleTypes, customErrorMessage)
        {
        }
    }
    
    /// <summary>
    /// MonoEntityTagFilter 的向後相容別名
    /// </summary>
    public class MonoEntityTagFilterAttribute : TypeRestrictFilterAttribute
    {
        public MonoEntityTagFilterAttribute(Type expectedEntityType, bool allowCompatibleTypes = true, string customErrorMessage = null)
            : base(expectedEntityType, allowCompatibleTypes, customErrorMessage)
        {
        }
    }
}
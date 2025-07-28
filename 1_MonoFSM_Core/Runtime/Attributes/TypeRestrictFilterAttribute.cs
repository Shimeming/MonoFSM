using System;

namespace _1_MonoFSM_Core.Runtime.Attributes
{
    /// <summary>
    /// 用於過濾帶有 RestrictType 屬性的欄位的通用 Attribute
    /// 可以指定期望的類型來過濾下拉選單中的選項
    /// 支援 VariableTag 和 MonoEntityTag 等類型
    /// </summary>
    public class TypeRestrictFilterAttribute : Attribute
    {
        /// <summary>
        /// 期望的類型
        /// </summary>
        public Type ExpectedType { get; }
        
        /// <summary>
        /// 是否允許相容的類型
        /// </summary>
        public bool AllowCompatibleTypes { get; }
        
        /// <summary>
        /// 自定義錯誤訊息
        /// </summary>
        public string CustomErrorMessage { get; }
        
        /// <summary>
        /// 構造函數
        /// </summary>
        /// <param name="expectedType">期望的類型</param>
        /// <param name="allowCompatibleTypes">是否允許相容的類型</param>
        /// <param name="customErrorMessage">自定義錯誤訊息</param>
        public TypeRestrictFilterAttribute(Type expectedType, bool allowCompatibleTypes = true, string customErrorMessage = null)
        {
            ExpectedType = expectedType;
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
                MonoFSM.Variable.VariableTag variableTag => variableTag.VariableMonoType,
                MonoFSM.Runtime.Mono.MonoEntityTag entityTag => entityTag.RestrictType,
                _ => null
            };
        }
        
        /// <summary>
        /// 獲取 Asset 搜尋過濾器
        /// </summary>
        public string GetAssetSearchFilter(Type targetType)
        {
            if (targetType == typeof(MonoFSM.Variable.VariableTag))
                return "t:VariableTag";
            if (targetType == typeof(MonoFSM.Runtime.Mono.MonoEntityTag))
                return "t:MonoEntityTag";
            
            // 通用處理：嘗試從類型名稱推斷
            return $"t:{targetType.Name}";
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
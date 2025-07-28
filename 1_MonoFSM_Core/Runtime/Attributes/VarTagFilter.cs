using System;

namespace _1_MonoFSM_Core.Runtime.Attributes
{
    /// <summary>
    /// 用於過濾 VariableTag 欄位的 Attribute
    /// 可以指定期望的變數類型來過濾下拉選單中的選項
    /// </summary>
    public class VarTagFilterAttribute : Attribute
    {
        /// <summary>
        /// 期望的變數類型（例如 VarFloat, VarInt 等）
        /// </summary>
        public Type ExpectedVariableType { get; }
        
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
        /// <param name="expectedVariableType">期望的變數類型</param>
        /// <param name="allowCompatibleTypes">是否允許相容的類型</param>
        /// <param name="customErrorMessage">自定義錯誤訊息</param>
        public VarTagFilterAttribute(Type expectedVariableType, bool allowCompatibleTypes = true, string customErrorMessage = null)
        {
            ExpectedVariableType = expectedVariableType;
            AllowCompatibleTypes = allowCompatibleTypes;
            CustomErrorMessage = customErrorMessage;
        }
    }
}
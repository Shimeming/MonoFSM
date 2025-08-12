using System;

namespace MonoFSM.Core.Attributes
{
    /// <summary>
    /// 標示該屬性或欄位的型別會根據VarTag的RestrictType動態決定
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class DynamicTypeAttribute : Attribute
    {
        /// <summary>
        /// 取得動態型別的方法名稱，預設為"GetDynamicValueType"
        /// </summary>
        public string TypeProviderMethod { get; set; } = "GetDynamicValueType";

        /// <summary>
        /// VarTag欄位名稱，預設為"_varTag"
        /// </summary>
        public string VarTagFieldName { get; set; } = "_varTag";

        public DynamicTypeAttribute() { }

        public DynamicTypeAttribute(string typeProviderMethod)
        {
            TypeProviderMethod = typeProviderMethod;
        }
    }
}

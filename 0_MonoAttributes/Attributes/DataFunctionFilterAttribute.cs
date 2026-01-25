using System;

namespace _1_MonoFSM_Core.Runtime.Attributes
{
    /// <summary>
    /// 用於過濾含有特定 DataFunction 的 GameData
    /// 例如：[DataFunctionFilter(typeof(PickableData))] 會只顯示有 PickableData 的 GameData
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DataFunctionFilterAttribute : Attribute
    {
        /// <summary>
        /// 要過濾的 DataFunction 類型
        /// </summary>
        public Type DataFunctionType { get; }

        /// <summary>
        /// 構造函數
        /// </summary>
        /// <param name="dataFunctionType">要過濾的 DataFunction 類型，必須繼承自 AbstractDataFunction</param>
        public DataFunctionFilterAttribute(Type dataFunctionType)
        {
            DataFunctionType = dataFunctionType;
        }
    }
}

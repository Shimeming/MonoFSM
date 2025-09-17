using System;

namespace MonoFSM.Core.DataProvider
{
    /// <summary>
    /// 提供 FieldPathEditor 根型別的介面
    /// 實作此介面的類別可以動態提供欄位路徑編輯的起始型別
    /// </summary>
    public interface IFieldPathRootTypeProvider
    {
        /// <summary>
        /// 獲取指定欄位的根型別
        /// </summary>
        /// <param name="fieldName">欄位名稱</param>
        /// <returns>該欄位的根型別，若無法確定則返回 null</returns>
        Type GetFieldPathRootType();
    }
}

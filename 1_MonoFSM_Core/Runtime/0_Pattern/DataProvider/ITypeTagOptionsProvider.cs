using System.Collections.Generic;
using MonoFSM.Variable.TypeTag;
using Sirenix.OdinInspector;

namespace MonoFSM.Core.DataProvider
{
    /// <summary>
    /// 提供 TypeTag 選項的介面
    /// 實作此介面的類別可以動態提供不同欄位的 TypeTag 選項
    /// </summary>
    public interface ITypeTagOptionsProvider
    {
        /// <summary>
        /// 獲取指定欄位的 TypeTag 選項
        /// </summary>
        /// <param name="fieldName">欄位名稱</param>
        /// <typeparam name="T">TypeTag 型別</typeparam>
        /// <returns>該欄位可用的 TypeTag 選項，若無法確定則返回空列表</returns>
        IEnumerable<ValueDropdownItem<T>> GetTypeTagOptions<T>(string fieldName)
            where T : AbstractTypeTag;
    }
}

using System.Collections.Generic;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.DataProvider;
using MonoFSM.Variable.TypeTag;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSM.Core.Test
{
    /// <summary>
    /// 測試 TypeTagSelectorAttribute 的範例類別
    /// </summary>
    public class TestTypeTagSelector : MonoBehaviour, ITypeTagOptionsProvider
    {
        [Header("範例1: 基本 Schema TypeTag 選擇")]
        [TypeTagSelector]
        public AbstractTypeTag _basicSchemaTag;

        [Header("範例2: 指定標題的 MonoTypeTag 選擇")]
        [TypeTagSelector("自訂 Mono 類型")]
        public MonoTypeTag _customMonoTag;

        [Header("範例3: 使用指定方法提供選項")]
        [TypeTagSelector("GetCustomOptions")]
        public AbstractTypeTag _customOptionsTag;

        [Header("範例4: 動態提供選項（透過介面）")]
        [TypeTagSelector]
        public VarMonoTypeTag _dynamicVarTag;

        [Header("範例5: 不顯示清空選項")]
        [TypeTagSelector(Title = "必選類型", ShowClearOption = false)]
        public MonoTypeTag _requiredTag;

        /// <summary>
        /// 提供自訂選項的方法範例
        /// </summary>
        public IEnumerable<ValueDropdownItem<AbstractTypeTag>> GetCustomOptions()
        {
            var options = new List<ValueDropdownItem<AbstractTypeTag>>();

            // 這裡可以根據業務邏輯返回不同的選項
            // 例如只返回特定條件下的 TypeTag

            return options;
        }

        /// <summary>
        /// 實作 ITypeTagOptionsProvider 介面
        /// 為不同欄位提供動態選項
        /// </summary>
        public IEnumerable<ValueDropdownItem<T>> GetTypeTagOptions<T>(string fieldName)
            where T : AbstractTypeTag
        {
            var options = new List<ValueDropdownItem<T>>();

            switch (fieldName)
            {
                case "_basicSchemaTag":
                    // 為 Schema TypeTag 提供特定選項
                    // 這裡可以根據當前上下文過濾選項
                    break;

                case "_dynamicVarTag":
                    // 為 Var TypeTag 提供動態選項
                    // 例如只顯示當前實體支援的變數類型
                    break;

                default:
                    // 預設情況下返回所有可用選項
                    break;
            }

            return options;
        }

        [Button("測試所有選項")]
        public void TestAllOptions()
        {
            Debug.Log($"Basic Schema Tag: {_basicSchemaTag?.name}");
            Debug.Log($"Custom Mono Tag: {_customMonoTag?.name}");
            Debug.Log($"Custom Options Tag: {_customOptionsTag?.name}");
            Debug.Log($"Dynamic Var Tag: {_dynamicVarTag?.name}");
            Debug.Log($"Required Tag: {_requiredTag?.name}");
        }
    }
}

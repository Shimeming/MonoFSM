using System;
using JetBrains.Annotations;

namespace MonoFSM.Core.Attributes
{
    /// <summary>
    /// 為 AbstractTypeTag 衍生類型欄位提供選擇介面
    /// </summary>
    [MeansImplicitUse]
    public class TypeTagSelectorAttribute : Attribute
    {
        /// <summary>
        /// 選項提供方法名稱，會在目標物件上尋找此方法來獲取選項
        /// </summary>
        public string OptionsProvider { get; set; }

        /// <summary>
        /// 是否使用 ITypeTagOptionsProvider 介面來動態獲取選項
        /// 當為 true 時，會優先檢查目標物件是否實作 ITypeTagOptionsProvider
        /// </summary>
        public bool UseDynamicProvider { get; set; } = true;

        /// <summary>
        /// 顯示標題文字
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 無選項時的提示訊息
        /// </summary>
        public string NoOptionsMessage { get; set; } = "無可用選項";

        /// <summary>
        /// 是否顯示清空選項
        /// </summary>
        public bool ShowClearOption { get; set; } = true;

        public TypeTagSelectorAttribute() { }

        /// <summary>
        /// 指定選項提供方法的建構子
        /// </summary>
        public TypeTagSelectorAttribute(string optionsProvider)
        {
            OptionsProvider = optionsProvider;
            UseDynamicProvider = false; // 明確指定時不使用動態提供者
        }

        /// <summary>
        /// 指定標題和選項提供方法的建構子
        /// </summary>
        public TypeTagSelectorAttribute(string title, string optionsProvider)
        {
            Title = title;
            OptionsProvider = optionsProvider;
            UseDynamicProvider = false;
        }

        /// <summary>
        /// 僅使用動態提供者的建構子
        /// </summary>
        public TypeTagSelectorAttribute(bool useDynamicProvider)
        {
            UseDynamicProvider = useDynamicProvider;
        }
    }
}

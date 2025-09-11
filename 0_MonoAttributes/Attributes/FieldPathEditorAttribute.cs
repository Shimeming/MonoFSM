using System;
using JetBrains.Annotations;

// using MonoFSM.Core.DataProvider;

namespace MonoFSM.Core.Attributes
{
    /// <summary>
    ///     為 List&lt;FieldPathEntry&gt; 欄位提供路徑編輯介面
    /// </summary>
    [MeansImplicitUse]
    public class FieldPathEditorAttribute : Attribute
    {
        /// <summary>
        ///     根型別名稱，用於路徑編輯的起始型別
        /// </summary>
        public string RootTypeName { get; set; }

        /// <summary>
        ///     根型別提供方法名稱，會在目標物件上尋找此方法來獲取根型別
        /// </summary>
        public string RootTypeProvider { get; set; }

        /// <summary>
        ///     是否使用 IFieldPathRootTypeProvider 介面來動態獲取根型別
        ///     當為 true 時，會優先檢查目標物件是否實作 IFieldPathRootTypeProvider
        /// </summary>
        public bool UseDynamicRootType { get; set; } = true;

        /// <summary>
        ///     無根型別時的錯誤訊息
        /// </summary>
        public string NoRootTypeMessage => "無法確定根型別";

        public FieldPathEditorAttribute() { }

        /// <summary>
        ///     指定根型別名稱的建構子
        /// </summary>
        public FieldPathEditorAttribute(string rootTypeName)
        {
            RootTypeName = rootTypeName;
            UseDynamicRootType = false; // 明確指定時不使用動態型別
        }

        /// <summary>
        ///     指定根型別提供方法的建構子
        /// </summary>
        public FieldPathEditorAttribute(string rootTypeProvider, bool isMethodProvider)
        {
            if (isMethodProvider)
                RootTypeProvider = rootTypeProvider;
            else
                RootTypeName = rootTypeProvider;
            UseDynamicRootType = false; // 明確指定時不使用動態型別
        }

        /// <summary>
        ///     僅使用動態根型別的建構子
        /// </summary>
        public FieldPathEditorAttribute(bool useDynamicRootType)
        {
            UseDynamicRootType = useDynamicRootType;
        }
    }
}

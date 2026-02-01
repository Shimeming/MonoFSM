using MonoFSM.Runtime;
using MonoFSM.Variable;
using UnityEngine;

namespace MonoFSM.Editor.VariableReferenceSystem
{
    /// <summary>
    /// 引用範圍：同一個 MonoEntity 或跨 Entity
    /// </summary>
    public enum ReferenceScope
    {
        Local,       // 同一個 MonoEntity 內
        CrossEntity  // 跨 MonoEntity
    }

    /// <summary>
    /// 引用方式
    /// </summary>
    public enum ReferenceType
    {
        DirectField,   // 直接欄位引用 (欄位類型繼承 AbstractMonoVariable)
        VarWrapper,    // 透過 VarWrapper 引用 (_var 欄位)
        ValueProvider  // 透過 ValueProvider 引用 (_varEntity + _varTag)
    }

    /// <summary>
    /// 描述一個 Variable 被引用的資訊
    /// </summary>
    public class VariableReferenceInfo
    {
        /// <summary>
        /// 被引用的目標 Variable
        /// </summary>
        public AbstractMonoVariable TargetVariable;

        /// <summary>
        /// 引用此 Variable 的 Component
        /// </summary>
        public Component ReferencingComponent;

        /// <summary>
        /// 欄位路徑 (例如: "_targetVar" 或 "_wrapper._var")
        /// </summary>
        public string FieldPath;

        /// <summary>
        /// 引用方式
        /// </summary>
        public ReferenceType Type;

        /// <summary>
        /// 引用範圍 (同 Entity / 跨 Entity)
        /// </summary>
        public ReferenceScope Scope;

        /// <summary>
        /// 引用來源所屬的 MonoEntity (ReferencingComponent 所屬的 Entity)
        /// </summary>
        public MonoEntity OwnerEntity;

        /// <summary>
        /// 取得顯示用的類型名稱
        /// </summary>
        public string TypeDisplayName
        {
            get
            {
                return Type switch
                {
                    ReferenceType.DirectField => "Direct",
                    ReferenceType.VarWrapper => "Wrapper",
                    ReferenceType.ValueProvider => "Provider",
                    _ => "Unknown"
                };
            }
        }

        /// <summary>
        /// 取得顯示用的 Component 名稱
        /// </summary>
        public string ComponentDisplayName
        {
            get
            {
                if (ReferencingComponent == null)
                    return "(null)";
                return ReferencingComponent.GetType().Name;
            }
        }
    }
}

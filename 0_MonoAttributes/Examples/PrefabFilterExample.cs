using _1_MonoFSM_Core.Runtime.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSM.Core.Examples
{
    /// <summary>
    /// PrefabFilterAttribute 使用範例
    /// 用於 MonoBehaviour 欄位，會過濾出包含該 Component 的 Prefab
    /// </summary>
    public class PrefabFilterExample : SerializedMonoBehaviour
    {
        [Header("基本Component過濾 - 任何Rigidbody")]
        [PrefabFilter]
        [SerializeField]
        private Rigidbody _rigidbodyComponent;

        [Header("需要特定額外Component的Prefab")]
        [PrefabFilter(
            typeof(Collider),
            customErrorMessage = "需要同時包含Rigidbody和Collider的Prefab"
        )]
        [SerializeField]
        private Rigidbody _rigidbodyWithCollider;

        [Header("自定義MonoBehaviour腳本")]
        [PrefabFilter]
        [SerializeField]
        private MonoBehaviour _customScript;

        [Header("需要特定Component且必須啟用")]
        [PrefabFilter(typeof(Renderer), onlyActiveGameObjects: true)]
        [SerializeField]
        private Collider _activeColliderWithRenderer;

        [Header("嚴格類型匹配（不允許繼承）")]
        [PrefabFilter(typeof(BoxCollider), allowInheritedTypes: false)]
        [SerializeField]
        private Transform _transformWithExactBoxCollider;
    }
}

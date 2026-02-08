using System.Collections.Generic;
using _1_MonoFSM_Core.Runtime._3_FlagData;
using MonoFSM.Utility;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSM.Core.SpawnTable
{
    /// <summary>
    /// 選取模式
    /// </summary>
    public enum SelectionMode
    {
        [Tooltip("依權重選一個")] Single,

        [Tooltip("依權重選 N 個")] PickN,

        [Tooltip("每個項目獨立判定機率")] Independent
    }

    /// <summary>
    /// SpawnTable 配置 - 支援權重選取、獨立機率、組合選取
    /// </summary>
    [CreateAssetMenu(menuName = "Config/SpawnTable", fileName = "SpawnTable")]
    public class SpawnTableConfig : AbstractSOConfig
    {
        [TableList(ShowIndexLabels = true)] [SerializeField]
        public List<SpawnTableEntry> _entries = new List<SpawnTableEntry>();

        [BoxGroup("Selection Settings")] [EnumToggleButtons] [SerializeField]
        public SelectionMode _selectionMode = SelectionMode.Single;

        [BoxGroup("Selection Settings")]
        [ShowIf("_selectionMode", SelectionMode.PickN)]
        [Min(1)]
        [Tooltip("要選取的項目數量")]
        [SerializeField]
        public int _pickCount = 1;

        [BoxGroup("Selection Settings")]
        [ShowIf("_selectionMode", SelectionMode.PickN)]
        [Tooltip("是否允許重複選取同一項目")]
        [SerializeField]
        public bool _allowDuplicates = false;

        /// <summary>
        /// 依據選取模式解析並返回選中的項目列表
        /// </summary>
        public List<SpawnTableEntry> Resolve()
        {
            if (_entries == null || _entries.Count == 0)
            {
                Debug.LogWarning($"SpawnTableConfig: {name} has no entries");
                return new List<SpawnTableEntry>();
            }

            // 過濾掉 prefab 為 null 的項目
            var validEntries = _entries.FindAll(e => e._prefab != null);
            if (validEntries.Count == 0)
            {
                Debug.LogWarning(
                    $"SpawnTableConfig: {name} has no valid entries (all prefabs are null)");
                return new List<SpawnTableEntry>();
            }

            switch (_selectionMode)
            {
                case SelectionMode.Single:
                    var single = WeightedRandomSelector.SelectOne(validEntries, e => e._weight);
                    return single != null
                        ? new List<SpawnTableEntry> { single }
                        : new List<SpawnTableEntry>();

                case SelectionMode.PickN:
                    return WeightedRandomSelector.SelectN(validEntries, _pickCount, e => e._weight,
                        _allowDuplicates);

                case SelectionMode.Independent:
                    return WeightedRandomSelector.SelectByProbability(validEntries,
                        e => e._probability);

                default:
                    Debug.LogError($"SpawnTableConfig: Unknown selection mode {_selectionMode}");
                    return new List<SpawnTableEntry>();
            }
        }

#if UNITY_EDITOR
        [BoxGroup("Preview")]
        [Button("測試 Resolve", ButtonSizes.Medium)]
        private void TestResolve()
        {
            var results = Resolve();
            Debug.Log($"[SpawnTable] {name} resolved {results.Count} entries:");
            foreach (var entry in results)
            {
                Debug.Log($"  - {entry._prefab.name} x{entry.GetSpawnCount()}");
            }
        }

        [BoxGroup("Preview")]
        [ShowInInspector]
        [ReadOnly]
        private float TotalWeight
        {
            get
            {
                if (_entries == null) return 0f;
                float total = 0f;
                foreach (var entry in _entries)
                    if (entry._prefab != null)
                        total += entry._weight;
                return total;
            }
        }
#endif
    }
}

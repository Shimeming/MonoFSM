using System.Collections.Generic;
using System.Linq;
using MonoFSM.Variable.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _1_MonoFSM_Core.Runtime._1_Conditions
{
    /// <summary>
    ///     階層式條件系統 - 從root往下的條件樹，葉節點只有當從root到該節點的所有路徑條件都符合時才成立
    /// </summary>
    public class HierarchicalCondition : AbstractConditionBehaviour
    {
        [Tooltip("這個節點的本地條件 - 必須滿足這個條件，加上所有父節點的條件")]
        [LabelText("本地條件")]
        [CompRef]
        [AutoChildren(DepthOneOnly = true)]
        private AbstractConditionBehaviour[] _localConditions;

        [Tooltip("是否為根節點 - 如果是，則不檢查父節點條件")]
        [LabelText("是否為根節點")]
        public bool IsRootNode;

        [ShowInInspector]
        [ReadOnly]
        [LabelText("從根節點到此節點的路徑條件")]
        private List<HierarchicalCondition> PathFromRoot
        {
            get
            {
                var path = new List<HierarchicalCondition>();
                BuildPathFromRoot(path);
                return path;
            }
        }

        public override string Description =>
            $"Hierarchical {_localConditions?.Length ?? 0} conditions";

        protected override bool IsValid
        {
            get
            {
                // 檢查本地條件
                if (_localConditions != null && !_localConditions.IsAllValid())
                    return false;

                // 如果是根節點，只需要檢查本地條件
                if (IsRootNode)
                    return true;

                // 檢查從根節點到這個節點的所有父節點條件
                return AreAllParentConditionsValid();
            }
        }

        /// <summary>
        ///     檢查從根節點到當前節點的所有父節點條件是否都滿足
        /// </summary>
        private bool AreAllParentConditionsValid()
        {
            var currentParent = transform.parent;

            while (currentParent != null)
            {
                var parentHierarchicalCondition =
                    currentParent.GetComponent<HierarchicalCondition>();
                if (parentHierarchicalCondition != null)
                {
                    // 檢查父節點的本地條件
                    if (
                        parentHierarchicalCondition._localConditions != null
                        && !parentHierarchicalCondition._localConditions.IsAllValid()
                    )
                        return false;

                    // 如果父節點是根節點，停止向上查找
                    if (parentHierarchicalCondition.IsRootNode)
                        break;
                }

                currentParent = currentParent.parent;
            }

            return true;
        }

        /// <summary>
        ///     建立從根節點到當前節點的路徑
        /// </summary>
        private void BuildPathFromRoot(List<HierarchicalCondition> path)
        {
            var currentParent = transform.parent;
            var parentPath = new List<HierarchicalCondition>();

            // 向上收集所有父節點
            while (currentParent != null)
            {
                var parentHierarchicalCondition =
                    currentParent.GetComponent<HierarchicalCondition>();
                if (parentHierarchicalCondition != null)
                {
                    parentPath.Add(parentHierarchicalCondition);

                    if (parentHierarchicalCondition.IsRootNode)
                        break;
                }

                currentParent = currentParent.parent;
            }

            // 反轉路徑，從根節點開始
            parentPath.Reverse();
            path.AddRange(parentPath);
            path.Add(this);
        }

        /// <summary>
        ///     獲取所有葉節點（沒有HierarchicalCondition子節點的節點）
        /// </summary>
        public List<HierarchicalCondition> GetLeafNodes()
        {
            var leafNodes = new List<HierarchicalCondition>();
            CollectLeafNodes(leafNodes);
            return leafNodes;
        }

        private void CollectLeafNodes(List<HierarchicalCondition> leafNodes)
        {
            var childHierarchicalConditions = GetComponentsInChildren<HierarchicalCondition>()
                .Where(h => h != this && h.transform.parent == transform)
                .ToArray();

            if (childHierarchicalConditions.Length == 0)
                // 這是葉節點
                leafNodes.Add(this);
            else
                // 繼續向下收集
                foreach (var child in childHierarchicalConditions)
                    child.CollectLeafNodes(leafNodes);
        }

#if UNITY_EDITOR
        [Button("顯示所有葉節點")]
        [TabGroup("Debug")]
        private void ShowAllLeafNodes()
        {
            if (!IsRootNode)
            {
                Debug.LogWarning("請在根節點上執行此操作", this);
                return;
            }

            var leafNodes = GetLeafNodes();
            Debug.Log($"找到 {leafNodes.Count} 個葉節點:", this);

            foreach (var leaf in leafNodes)
            {
                var pathFromRoot = leaf.PathFromRoot;
                var pathNames = pathFromRoot.Select(p => p.name).ToArray();
                Debug.Log(
                    $"葉節點: {leaf.name}, 路徑: {string.Join(" -> ", pathNames)}, 狀態: {leaf.FinalResult}",
                    leaf
                );
            }
        }

        [Button("測試路徑條件")]
        [TabGroup("Debug")]
        private void TestPathConditions()
        {
            var path = PathFromRoot;
            Debug.Log($"從根節點到 {name} 的路徑:", this);

            for (var i = 0; i < path.Count; i++)
            {
                var node = path[i];
                var localValid = node._localConditions?.IsAllValid() ?? true;
                Debug.Log($"  {i}: {node.name} - 本地條件: {localValid}", node);
            }

            Debug.Log($"最終結果: {FinalResult}", this);
        }
#endif

        public override bool IsDrawingValueInfo => true;
        public override string ValueInfo => _note;
    }
}

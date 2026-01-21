using System;
using System.Collections.Generic;
using System.Linq;
using _1_MonoFSM_Core.Runtime.Attributes;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MonoFSM.Core.Editor
{
    /// <summary>
    /// PrefabFilterAttribute 的 OdinAttributeDrawer
    /// 用於過濾帶有特定Component的Prefab
    /// </summary>
    public class PrefabFilterAttributeDrawer : OdinAttributeDrawer<PrefabFilterAttribute>
    {
        protected override bool CanDrawAttributeProperty(InspectorProperty property)
        {
            return property.ValueEntry != null
                && typeof(MonoBehaviour).IsAssignableFrom(property.ValueEntry.TypeOfValue);
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            var currentComponent = Property.ValueEntry.WeakSmartValue as MonoBehaviour;

            // var currentPrefab = currentComponent?.gameObject;
            //
            // // 驗證當前選中的Prefab是否符合過濾條件
            // if (currentPrefab != null && !Attribute.ValidatePrefab(currentPrefab))
            // {
            //     var warningMessage = !string.IsNullOrEmpty(Attribute.CustomErrorMessage)
            //         ? Attribute.CustomErrorMessage
            //         : GetDefaultWarningMessage(currentPrefab);
            //
            //     SirenixEditorGUI.WarningMessageBox(warningMessage);
            // }

            // 繪製帶過濾功能的選擇器
            DrawFilteredSelector(label, currentComponent);

            // 設置背景顏色
            GUI.backgroundColor =
                Property.ValueEntry.WeakSmartValue == null
                    ? new Color(0.2f, 0.2f, 0.3f, 0.1f)
                    : new Color(0.1f, 0.3f, 0.2f, 0.2f);

            var newComponent =
                SirenixEditorFields.UnityObjectField(
                    currentComponent,
                    Property.ValueEntry.TypeOfValue,
                    false
                ) as MonoBehaviour;

            Property.ValueEntry.WeakSmartValue = newComponent;
            GUI.backgroundColor = Color.white;
        }

        private void DrawFilteredSelector(GUIContent label, MonoBehaviour currentValue)
        {
            var buttonText = currentValue != null ? currentValue.name : "None";

            using (new GUILayout.HorizontalScope())
            {
                if (label != null)
                    EditorGUILayout.PrefixLabel(label);

                if (
                    SirenixEditorGUI.SDFIconButton(
                        buttonText,
                        16,
                        SdfIconType.CaretDownFill,
                        IconAlignment.RightEdge
                    )
                )
                {
                    var selector = new PrefabFilteredSelector(
                        Attribute,
                        Property.ValueEntry.TypeOfValue
                    );
                    selector.SelectionConfirmed += col =>
                    {
                        var selectedPrefab = col.FirstOrDefault();
                        if (selectedPrefab != null)
                        {
                            // 從選中的 Prefab 中獲取對應類型的 Component
                            var component = selectedPrefab.GetComponent(
                                Property.ValueEntry.TypeOfValue
                            );
                            Property.ValueEntry.WeakSmartValue = component;
                        }
                        else
                        {
                            Property.ValueEntry.WeakSmartValue = null;
                        }
                    };
                    selector.ShowInPopup();
                }
            }
        }

        private string GetDefaultWarningMessage(GameObject prefab)
        {
            if (Attribute.RequiredComponentType == null)
                return $"選中的Prefab '{prefab.name}' 不符合條件";

            var componentName = Attribute.RequiredComponentType.Name;
            return $"選中的Prefab '{prefab.name}' 缺少必要的Component: {componentName}";
        }
    }

    /// <summary>
    /// Prefab過濾選擇器
    /// </summary>
    public class PrefabFilteredSelector : OdinSelector<GameObject>
    {
        private readonly PrefabFilterAttribute _filterAttribute;
        private readonly Type _componentType;

        public PrefabFilteredSelector(
            PrefabFilterAttribute filterAttribute,
            Type componentType = null
        )
        {

            _filterAttribute = filterAttribute;
            _componentType = componentType;
            DrawConfirmSelectionButton = false;
            SelectionTree.Config.SelectMenuItemsOnMouseDown = true;
            SelectionTree.Config.ConfirmSelectionOnDoubleClick = true;
        }

        protected override void BuildSelectionTree(OdinMenuTree tree)
        {
            tree.Config.DrawSearchToolbar = true;

            // 添加 None 選項
            tree.Add("-- None --", null);

            // 獲取過濾後的Prefab
            var filteredPrefabs = GetFilteredPrefabs().ToList();

            if (!filteredPrefabs.Any())
            {
                tree.Add("無符合條件的Prefab", null);
                return;
            }

            // 按資料夾分組
            var groupedPrefabs = filteredPrefabs
                .Where(prefab => prefab != null)
                .GroupBy(prefab => GetGroupName(prefab))
                .OrderBy(g => g.Key)
                .ToList();

            foreach (var group in groupedPrefabs)
            {
                var sortedPrefabs = group.OrderBy(prefab => prefab.name).ToList();

                foreach (var prefab in sortedPrefabs)
                {
                    var displayName = prefab.name;
                    var path = group.Key == "Assets" ? displayName : $"{group.Key}/{displayName}";

                    tree.Add(path, prefab);
                }
            }
        }

        private IEnumerable<GameObject> GetFilteredPrefabs()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            using var searchContext = SearchService.CreateContext(new[] { "asset" }, "t:MonoObj");
            using var results = SearchService.Request(searchContext, SearchFlags.Synchronous);
            {
                return results
                    .Select(r => r.ToObject<GameObject>())
                    // .Where(prefab => prefab != null && ValidatePrefab(prefab))
                    .ToList();
            }
            return null;
            // var guids = AssetDatabase.FindAssets(searchFilter);
            // Debug.Log($"[PrefabFilter] Found {guids.Length} assets with filter: {searchFilter}");
            // var results = new List<GameObject>();
            //
            // var loadCount = 0;
            // var validateCount = 0;
            // var validCount = 0;
            // var skipCount = 0;
            //
            // foreach (var guid in guids)
            // {
            //     var path = AssetDatabase.GUIDToAssetPath(guid);
            //
            //     // 早期過濾：檢查路徑是否為.prefab檔案
            //     if (!path.EndsWith(".prefab"))
            //     {
            //         skipCount++;
            //         continue;
            //     }
            //
            //     var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            //     loadCount++;
            //
            //     if (prefab != null) // && ValidatePrefab(prefab))
            //     {
            //         results.Add(prefab);
            //         validCount++;
            //     }
            //
            //     validateCount++;
            // }
            //
            // stopwatch.Stop();
            // Debug.Log($"[PrefabFilter] 效能報告:\n" +
            //           $"- 搜尋到 {guids.Length} 個資產\n" +
            //           $"- 跳過非Prefab: {skipCount} 個\n" +
            //           $"- LoadAsset: {loadCount} 次\n" +
            //           $"- 驗證: {validateCount} 次\n" +
            //           $"- 有效: {validCount} 個\n" +
            //           $"- 總耗時: {stopwatch.ElapsedMilliseconds} ms\n" +
            //           $"- 平均每個Load: {(loadCount > 0 ? (float)stopwatch.ElapsedMilliseconds / loadCount : 0):F2} ms");
            //
            // return results;
        }

        private bool ValidatePrefab(GameObject prefab)
        {
            if (prefab == null)
                return false;

            // 確保是Prefab而不是場景中的GameObject（已在上層過濾，這裡可以省略）
            // var assetPath = AssetDatabase.GetAssetPath(prefab);
            // if (string.IsNullOrEmpty(assetPath) || !assetPath.EndsWith(".prefab"))
            //     return false;

            // 檢查是否包含所需的 Component 類型
            if (_componentType != null)
            {
                var component = prefab.GetComponent(_componentType);
                if (component == null)
                    return false;
            }

            return _filterAttribute.ValidatePrefab(prefab);
        }

        private string GetGroupName(GameObject prefab)
        {
            var assetPath = AssetDatabase.GetAssetPath(prefab);
            if (string.IsNullOrEmpty(assetPath))
                return "Unknown";

            var folderPath = System.IO.Path.GetDirectoryName(assetPath);
            return string.IsNullOrEmpty(folderPath) ? "Assets" : folderPath;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using _1_MonoFSM_Core.Runtime._1_States;
using MonoFSM.Core.Editor.Utility;
using MonoFSM.Variable.TypeTag;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MonoFSM.Variable.SOType
{
    /// <summary>
    ///     Schema TypeTag 管理器
    ///     負責管理所有 AbstractEntitySchema 類型對應的 MonoTypeTag
    ///     提供自動查找、創建和映射功能
    /// </summary>
    [CreateAssetMenu(
        fileName = "SchemaTypeTagManager",
        menuName = "MonoFSM/Manager/Schema TypeTag Manager"
    )]
    public class SchemaTypeTagManager : ScriptableObjectSingleton<SchemaTypeTagManager>
    {
        [SerializeField]
        [InfoBox("管理所有 Schema 類型與 TypeTag 的映射關係")]
        private List<SchemaTypeMapping> _mappings = new();

        [ShowInInspector]
        [ReadOnly]
        [InfoBox("當前映射統計")]
        private string MappingStats => $"已映射 {_mappings.Count} 個 Schema 類型";

        /// <summary>
        ///     根據 Schema 類型獲取或創建對應的 TypeTag
        /// </summary>
        public MonoTypeTag GetOrCreateTypeTagForSchema<T>()
            where T : AbstractEntitySchema
        {
            return GetOrCreateTypeTagForSchema(typeof(T));
        }

        /// <summary>
        ///     根據 Schema 類型獲取或創建對應的 TypeTag
        /// </summary>
        public MonoTypeTag GetOrCreateTypeTagForSchema(Type schemaType)
        {
            if (!typeof(AbstractEntitySchema).IsAssignableFrom(schemaType))
            {
                Debug.LogError($"類型 {schemaType.Name} 不是 AbstractEntitySchema 的子類", this);
                return null;
            }

            // 1. 先從 mapping 中查找
            var existingMapping = _mappings.FirstOrDefault(m => m.SchemaType == schemaType);
            if (existingMapping?._typeTag != null)
            {
                Debug.Log(
                    $"從映射中找到 TypeTag: {existingMapping._typeTag.name} for {schemaType.Name}",
                    this
                );
                return existingMapping._typeTag;
            }

#if UNITY_EDITOR
            // 2. 用 SOUtility 搜尋是否已有對應的 TypeTag
            var existingTypeTag = SOUtility
                .GetAllAssetsOfType<MonoTypeTag>()
                .FirstOrDefault(tag => tag.Type == schemaType);

            if (existingTypeTag != null)
            {
                Debug.Log(
                    $"找到現有的 TypeTag: {existingTypeTag.name} for {schemaType.Name}",
                    this
                );
                // 更新 mapping 記錄
                UpdateMapping(schemaType, existingTypeTag);
                return existingTypeTag;
            }

            // 3. 如果都沒有，自動創建新的 TypeTag
            Debug.Log($"未找到對應的 TypeTag，將為 {schemaType.Name} 創建新的 TypeTag", this);
            return CreateNewTypeTag(schemaType);
#else
            Debug.LogWarning($"在 Runtime 模式下無法創建新的 TypeTag for {schemaType.Name}", this);
            return null;
#endif
        }

        /// <summary>
        ///     獲取已存在的 TypeTag（不創建新的）
        /// </summary>
        public MonoTypeTag GetExistingTypeTag(Type schemaType)
        {
            // 從 mapping 中查找
            var existingMapping = _mappings.FirstOrDefault(m => m.SchemaType == schemaType);
            if (existingMapping?._typeTag != null)
                return existingMapping._typeTag;

#if UNITY_EDITOR
            // 用 SOUtility 搜尋
            return SOUtility
                .GetAllAssetsOfType<MonoTypeTag>()
                .FirstOrDefault(tag => tag.Type == schemaType);
#else
            return null;
#endif
        }

#if UNITY_EDITOR
        private MonoTypeTag CreateNewTypeTag(Type schemaType)
        {
            // 創建新的 MonoTypeTag
            var newTypeTag = CreateInstance<MonoTypeTag>();

            // 設定類型 - 創建泛型 MySerializedType，並設定 RestrictType
            if (newTypeTag._type == null)
                // 創建 MySerializedType<MonoBehaviour> 實例
                newTypeTag._type = new MySerializedType<MonoBehaviour>();

            // 直接設定 RestrictType 為對應的 Schema 類型
            newTypeTag._type.RestrictType = schemaType;

            Debug.Log($"設定 TypeTag 的 RestrictType: {schemaType.Name}", newTypeTag);

            // 設定檔案名稱
            var fileName = $"[Type] {schemaType.Name}";

            // 決定存放路徑
            var savePath = "Assets/10_Scriptables/VarMonoTypeTags";

            // 確保資料夾存在
            if (!AssetDatabase.IsValidFolder(savePath))
            {
                var parentPath = "Assets/10_Scriptables";
                if (!AssetDatabase.IsValidFolder(parentPath))
                    AssetDatabase.CreateFolder("Assets", "10_Scriptables");
                AssetDatabase.CreateFolder(parentPath, "VarMonoTypeTags");
            }

            // 創建資產
            var fullPath = $"{savePath}/{fileName}.asset";
            AssetDatabase.CreateAsset(newTypeTag, fullPath);

            // 觸發 OnBeforeSceneSave 來自動重命名檔案
            newTypeTag.OnBeforeSceneSave();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"創建新的 TypeTag: {fullPath}，RestrictType: {schemaType.Name}", newTypeTag);

            // 更新映射
            UpdateMapping(schemaType, newTypeTag);

            return newTypeTag;
        }

        private void UpdateMapping(Type schemaType, MonoTypeTag typeTag)
        {
            // 檢查是否已有映射
            var existingMapping = _mappings.FirstOrDefault(m => m.SchemaType == schemaType);

            if (existingMapping != null)
                // 更新現有映射
                existingMapping._typeTag = typeTag;
            else
                // 創建新映射
                _mappings.Add(
                    new SchemaTypeMapping
                    {
                        _schemaTypeName = schemaType.AssemblyQualifiedName,
                        _typeTag = typeTag,
                    }
                );

            // 標記為 dirty 以便存檔
            EditorUtility.SetDirty(this);
            Debug.Log($"更新映射: {schemaType.Name} -> {typeTag.name}", this);
        }

        [Button("刷新所有映射")]
        [InfoBox("掃描專案中所有的 Schema 類型和 TypeTag，建立完整的映射關係")]
        private void RefreshAllMappings()
        {
            var allSchemaTypes = AppDomain
                .CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type =>
                    typeof(AbstractEntitySchema).IsAssignableFrom(type)
                    && !type.IsAbstract
                    && !type.IsInterface
                )
                .ToList();

            var allTypeTags = SOUtility.GetAllAssetsOfType<MonoTypeTag>().ToList();

            Debug.Log(
                $"找到 {allSchemaTypes.Count} 個 Schema 類型，{allTypeTags.Count} 個 TypeTag",
                this
            );

            // 為每個 Schema 類型尋找對應的 TypeTag
            foreach (var schemaType in allSchemaTypes)
            {
                var matchingTypeTag = allTypeTags.FirstOrDefault(tag => tag.Type == schemaType);
                if (matchingTypeTag != null)
                    UpdateMapping(schemaType, matchingTypeTag);
            }

            Debug.Log($"刷新完成，當前有 {_mappings.Count} 個映射", this);
        }

        [Button("清理無效映射")]
        private void CleanInvalidMappings()
        {
            var originalCount = _mappings.Count;

            _mappings.RemoveAll(mapping => mapping._typeTag == null || mapping.SchemaType == null);

            var removedCount = originalCount - _mappings.Count;

            if (removedCount > 0)
            {
                EditorUtility.SetDirty(this);
                Debug.Log($"清理了 {removedCount} 個無效映射", this);
            }
            else
            {
                Debug.Log("沒有發現無效映射", this);
            }
        }
#endif

        [Button("顯示所有映射")]
        private void ShowAllMappings()
        {
            Debug.Log("=== Schema TypeTag 映射列表 ===", this);
            foreach (var mapping in _mappings)
            {
                var typeName = mapping.SchemaType?.Name ?? "Unknown";
                var tagName = mapping._typeTag?.name ?? "Missing";
                Debug.Log($"{typeName} -> {tagName}", this);
            }

            Debug.Log($"總計: {_mappings.Count} 個映射", this);
        }
    }

    /// <summary>
    ///     Schema 類型與 TypeTag 的映射關係
    /// </summary>
    [Serializable]
    public class SchemaTypeMapping
    {
        [HideInInspector]
        public string _schemaTypeName; // 用於序列化，因為 Type 不能直接序列化

        public MonoTypeTag _typeTag;

        [ShowInInspector]
        [ReadOnly]
        public string SchemaTypeName => SchemaType?.Name ?? "Unknown Type";

        public Type SchemaType
        {
            get
            {
                if (string.IsNullOrEmpty(_schemaTypeName))
                    return null;

                try
                {
                    return Type.GetType(_schemaTypeName);
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}

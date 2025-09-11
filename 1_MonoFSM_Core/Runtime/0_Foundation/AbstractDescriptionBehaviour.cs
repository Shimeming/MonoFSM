using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using MonoFSM.Core;
using MonoFSM.Core.Attributes;
using MonoFSM.EditorExtension;
using MonoFSM.Runtime;
using MonoFSM.Runtime.Attributes;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace MonoFSM.Foundation
{
    // [InfoBox("$_errorMessage", InfoMessageType.Error, "$HasError")]
    public abstract class AbstractDescriptionBehaviour
        : MonoBehaviour,
            IBeforePrefabSaveCallbackReceiver,
            IAfterPrefabStageOpenCallbackReceiver,
            IDrawHierarchyBackGround
    {
#if UNITY_EDITOR
        [TextArea]
        [SerializeField]
        protected string _note; //這個應該要有另外的地方可以draw? 多component還會打架...
#endif

        /// <summary>
        ///     想要自己命名，又要帶有description tag的話就把Description改用這個
        /// </summary>
        protected string ReformatedName => FormatName(name);

        public string FormatName(string str)
        {
            return Regex.Replace(str, @"\[.*?\]", "").Trim();
        }

        // Cache for required fields per type (serialized only)
        private static readonly Dictionary<Type, FieldInfo[]> _requiredFieldsCache = new();

        // Cache for required fields per type (including non-serialized)
        private static readonly Dictionary<
            Type,
            FieldInfo[]
        > _requiredFieldsCacheWithNonSerialized = new();

        // Track if we're in prefab stage mode for more detailed error checking
        private bool _isPrefabStageMode = false;

        //沒有做AutoComponent下會顯示error? 還是應該讓prefab openstage時做一次，scene上跳過這個判定，雖然稍嫌trivial
        private static FieldInfo[] GetRequiredHierarchyValidateFields(
            Type type,
            bool includeNonSerialized = false
        )
        {
            var cache = includeNonSerialized
                ? _requiredFieldsCacheWithNonSerialized
                : _requiredFieldsCache;

            if (cache.TryGetValue(type, out var cachedFields))
                return cachedFields;

            var fields = type.GetFields(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
            );

            // Find all fields with [Required] or [DropDownRef] attributes that are not "interfaces"
            //interface在組合component就會看到了, 也比較不會在refactor之後掉reference
            FieldInfo[] requiredFields;

            if (includeNonSerialized)
                // Include all fields with required attributes, regardless of serialization
                requiredFields = Array.FindAll(
                    fields,
                    f =>
                        (
                            f.GetCustomAttributes(typeof(RequiredAttribute), false).Length > 0
                            || f.GetCustomAttributes(typeof(DropDownRefAttribute), false).Length > 0
                        ) && !f.FieldType.IsInterface
                );
            else
                // Only include public or SerializeField attributes (original behavior)
                requiredFields = Array.FindAll(
                    fields,
                    f =>
                        (
                            f.GetCustomAttribute(typeof(RequiredAttribute), false) != null
                            || f.GetCustomAttribute(typeof(DropDownRefAttribute), false) != null
                        )
                        && !f.FieldType.IsInterface
                        && (
                            f.IsPublic
                            || f.GetCustomAttribute(typeof(SerializeField), false) != null
                        )
                );

            cache[type] = requiredFields;

            return requiredFields;
        }

        //FIXME: 應該改成required attribute processor, 然後配合interface做drawHierarchy?
        //用reflection找到所有[Required]的field，然後檢查是否有null
        private bool CheckNullOfRequiredFields()
        {
            var requiredFields = GetRequiredHierarchyValidateFields(GetType());
            foreach (var field in requiredFields)
            {
                // 檢查是否應該驗證此欄位（根據條件方法）
                if (!ShouldValidateField(field))
                    continue;

                // Debug.Log($"Checking required field: {field.Name} in {gameObject.name}", this);
                var value = field.GetValue(this) as Object;
                if (value == null)
                {
                    _errorMessage = $"Required field '{field.Name}' is null in {gameObject.name}";
                    // Debug.LogError($"Required field '{field.Name}' is null in {gameObject.name}");
                    // UnityEditor.EditorGUIUtility.PingObject(this);
                    return true;
                }
            }

            // Debug.Log($"All required fields are set in {gameObject.name}");
            _errorMessage = "pass!";

            return false;
        }

        // Prefab stage specific error checking that includes non-serialized fields
        private bool CheckNullOfRequiredFieldsForPrefabStage(bool isShowError = false)
        {
            // var requiredFields = GetRequiredHierarchyValidateFields(GetType(), true);
            var requiredFields = GetRequiredHierarchyValidateFields(GetType());
            foreach (var field in requiredFields)
            {
                // Debug.Log(
                //     $"Prefab Stage Checking required field: {field.Name} in {gameObject.name}",
                // this);
                // 檢查是否應該驗證此欄位（根據條件方法）
                if (!ShouldValidateField(field))
                    continue;

                var value = field.GetValue(this) as Object;
                if (value == null)
                {
                    _errorMessage =
                        $"Required field '{field.Name}' is null in {gameObject.name} (Prefab Stage Check)";
                    //FIXME: 一打開prefab想log?

                    if (isShowError)
                    {
                        Debug.LogError(
                            $"Required field '{field.Name}' is null in {gameObject.name}",
                            this
                        );
                        EditorGUIUtility.PingObject(this);
                    }

                    return true;
                }
            }

            _errorMessage = "pass!";
            return false;
        }

        /// <summary>
        /// 檢查是否應該驗證指定的欄位（根據 ValueTypeValidateAttribute 的條件方法）
        /// </summary>
        private bool ShouldValidateField(FieldInfo field)
        {
            // 獲取欄位上的 ValueTypeValidateAttribute
            var validateAttribute = field.GetCustomAttribute<ValueTypeValidateAttribute>();
            if (
                validateAttribute == null
                || string.IsNullOrEmpty(validateAttribute.ConditionalMethod)
            )
                return true; // 沒有條件方法，總是驗證

            try
            {
                // 嘗試調用條件方法
                var method = GetType()
                    .GetMethod(
                        validateAttribute.ConditionalMethod,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );

                if (method == null)
                {
                    Debug.LogWarning(
                        $"找不到條件方法 '{validateAttribute.ConditionalMethod}' 在型別 {GetType().Name} 中，欄位 '{field.Name}'"
                    );
                    return true; // 找不到方法時預設為驗證
                }

                // 檢查方法返回型別是否為 bool
                if (method.ReturnType != typeof(bool))
                {
                    Debug.LogWarning(
                        $"條件方法 '{validateAttribute.ConditionalMethod}' 必須返回 bool 型別，欄位 '{field.Name}'"
                    );
                    return true;
                }

                // 調用方法並返回結果
                var result = method.Invoke(this, null);
                return result is bool boolResult && boolResult;
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"執行條件方法 '{validateAttribute.ConditionalMethod}' 時發生錯誤: {ex.Message}，欄位 '{field.Name}'"
                );
                return true; // 發生錯誤時預設為驗證
            }
        }

        [AutoParent]
        protected MonoEntity _self; //FIXME: 每個都要嗎？

        public MonoEntity ParentEntity => _self;

        //介面上也顯示？textarea?
        public virtual string Description => GetType().Name;

        protected virtual string DescriptionPreprocess(string text) => text;

        protected abstract string DescriptionTag { get; }

        [InfoBox("$Description")]
        [HideInInlineEditors]
        [Button]
        protected void Rename()
        {
            // gameObject.name = $"[Action] {GetType().Name.Split("Action")[0]} {renamePostfix}";
#if UNITY_EDITOR
            try
            {
                // Debug.Log("DescriptionTag: " + DescriptionTag, this);
                // Debug.Log(
                //     $"Description of {GetType()}: Description:{Description} process:{DescriptionPreprocess(Description)}",
                //     this);
                if (IsBracketsNeededForTag)
                    gameObject.name = $"[{DescriptionTag}] {DescriptionPreprocess(Description)}";
                else
                    gameObject.name = $"{DescriptionTag} {DescriptionPreprocess(Description)}";
                EditorUtility.SetDirty(gameObject);
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"Error renaming gameObject: {gameObject.name} to [{DescriptionTag}]",
                    this
                );
            }

#endif
        }

        protected virtual bool IsBracketsNeededForTag => true;

        protected virtual void Awake() { }

        protected virtual void Start() { }

        [Button("Save Process")]
        public virtual void OnBeforePrefabSave()
        {
#if UNITY_EDITOR
            AutoAttributeManager.AutoReference(this); //有些field需要autoChildren容易造成 description null
            CheckNullOfRequiredFieldsForPrefabStage(true);
            Rename();
#endif
        }

        public virtual void OnAfterPrefabStageOpen()
        {
            _isPrefabStageMode = true;
            // Trigger error checking with non-serialized fields included
            CheckNullOfRequiredFieldsForPrefabStage(true);
        }

        // [InfoBox("$_errorMessage", InfoMessageType.Error, "$HasError")]
        protected virtual bool HasError()
        {
            // Use different checking logic based on environment
            var isInPrefabStage = _isPrefabStageMode;

#if UNITY_EDITOR
            // Double-check using Unity's API for more reliability
            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                isInPrefabStage = true;

            return isInPrefabStage
                ? CheckNullOfRequiredFieldsForPrefabStage()
                : CheckNullOfRequiredFields();
#else
            return false;
#endif
        }

        [InfoBox("$_errorMessage", InfoMessageType.Error, "$HasError")]
        [PreviewInDebugMode]
        protected string _errorMessage;

        public Color BackgroundColor => new(1.0f, 0f, 0f, 0.3f);

        [ShowInDebugMode]
        public bool IsDrawGUIHierarchyBackground => !Application.isPlaying && HasError(); //還是用icon?
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using MonoFSM.Core.DataProvider;
using MonoFSM.Runtime.Attributes;
using MonoFSM.Variable;
using MonoFSM.Variable.FieldReference;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace MonoFSM.Core.Editor
{
    /// <summary>
    /// ValueTypeValidateAttribute 的 OdinAttributeDrawer
    /// 用於檢測和顯示 IValueProvider 的 ValueType 驗證結果
    /// </summary>
    [UsedImplicitly]
    [DrawerPriority(1, 1, 0)] //沒搞懂XDD
    // [DrawerPriority(0.0, 2.0, 0.25)]
    public class ValueTypeValidateDrawer : OdinAttributeDrawer<ValueTypeValidateAttribute>
    {
        protected override bool CanDrawAttributeProperty(InspectorProperty property)
        {
            // 只處理有值的屬性
            return property.ValueEntry != null;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            // 檢查是否有條件方法，如果有且條件不滿足，則跳過驗證
            if (!ShouldValidate())
            {
                CallNextDrawer(label);
                return;
            }

            var provider = Property.ValueEntry?.WeakSmartValue as IValueProvider;
            if (provider == null)
            {
                DisplayErrorAndCallNextDrawer("null provider (not valueProvider)", label);
                return;
            }
            // Debug.Log("provider" + Property.Name + (provider == null) + provider.ValueType);
            if (provider.ValueType == null)
            {
                DisplayErrorAndCallNextDrawer("provider.valuetype is null", label);
                return;
            }

            var result = ValidateValueType(provider);
            DisplayValidationResultAndCallNextDrawer(result, provider, label);
        }

        /// <summary>
        /// 檢查是否應該執行驗證（根據條件方法）
        /// </summary>
        private bool ShouldValidate()
        {
            // 如果沒有指定條件方法，則總是驗證
            if (string.IsNullOrEmpty(Attribute.ConditionalMethod))
                return true;

            // 獲取父物件
            var target = Property.ParentValues.FirstOrDefault();
            if (target == null)
                return true; // 如果無法獲取目標物件，預設為驗證

            try
            {
                // 嘗試調用條件方法
                var method = target
                    .GetType()
                    .GetMethod(
                        Attribute.ConditionalMethod,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );

                if (method == null)
                {
                    Debug.LogWarning(
                        $"找不到條件方法 '{Attribute.ConditionalMethod}' 在型別 {target.GetType().Name} 中"
                    );
                    return true; // 找不到方法時預設為驗證
                }

                // 檢查方法返回型別是否為 bool
                if (method.ReturnType != typeof(bool))
                {
                    Debug.LogWarning(
                        $"條件方法 '{Attribute.ConditionalMethod}' 必須返回 bool 型別"
                    );
                    return true;
                }

                // 調用方法並返回結果
                var result = method.Invoke(target, null);
                return result is bool boolResult && boolResult;
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"執行條件方法 '{Attribute.ConditionalMethod}' 時發生錯誤: {ex.Message}"
                );
                return true; // 發生錯誤時預設為驗證
            }
        }

        /// <summary>
        /// 取得 Provider 錯誤訊息
        /// </summary>
        private string GetProviderErrorMessage()
        {
            return Property.ValueEntry?.WeakSmartValue != null
                ? "此欄位不是 IValueProvider 類型，無法進行 ValueType 驗證"
                : "此欄位為空，無法進行 ValueType 驗證";
        }

        /// <summary>
        /// 顯示錯誤訊息並調用下一個繪製器
        /// </summary>
        private void DisplayErrorAndCallNextDrawer(string errorMessage, GUIContent label)
        {
            SirenixEditorGUI.ErrorMessageBox(errorMessage);
            CallNextDrawer(label);
        }

        /// <summary>
        /// 顯示驗證結果並調用下一個繪製器
        /// </summary>
        private void DisplayValidationResultAndCallNextDrawer(
            TypeValidationResult result,
            IValueProvider provider,
            GUIContent label
        )
        {
            if (!result.IsValid)
            {
                var errorMessage = BuildErrorMessage(result);
                DisplayErrorAndCallNextDrawer(errorMessage, label);
            }
            else if (!string.IsNullOrEmpty(result.WarningMessage))
            {
                SirenixEditorGUI.WarningMessageBox(result.WarningMessage);
                CallNextDrawer(label);
            }
            else if (Attribute.ShowSuccessMessage)
            {
                DrawWithSuccessColor(CreateSuccessLabel(provider, label));
            }
            else
            {
                CallNextDrawer(label);
            }
        }

        /// <summary>
        /// 建立錯誤訊息
        /// </summary>
        private string BuildErrorMessage(TypeValidationResult result)
        {
            var errorMessage = result.ErrorMessage;
            if (result.Suggestions != null && result.Suggestions.Count > 0)
            {
                errorMessage += "\n建議：" + string.Join("、", result.Suggestions.ToArray());
            }
            return errorMessage;
        }

        /// <summary>
        /// 創建成功標籤
        /// </summary>
        private GUIContent CreateSuccessLabel(IValueProvider provider, GUIContent originalLabel)
        {
            var tooltip = $"ValueType 驗證成功：{provider.ValueType?.Name ?? "Unknown"}";
            var modifiedLabel =
                originalLabel != null ? new GUIContent(originalLabel) : new GUIContent();
            modifiedLabel.text = "✓ " + (modifiedLabel.text ?? "");
            modifiedLabel.tooltip = string.IsNullOrEmpty(modifiedLabel.tooltip)
                ? tooltip
                : modifiedLabel.tooltip + " | " + tooltip;

            return modifiedLabel;
        }

        /// <summary>
        /// 以綠色繪製成功訊息
        /// </summary>
        private void DrawWithSuccessColor(GUIContent label)
        {
            var oldColor = GUI.color;
            GUI.color = new Color(0.3f, 0.8f, 0.3f, 1f);
            CallNextDrawer(label);
            GUI.color = oldColor;
        }

        /// <summary>
        /// 驗證型別的輔助方法（支援 ValueType 和 VariableType 驗證）
        /// </summary>
        private TypeValidationResult ValidateValueType(IValueProvider provider)
        {
            if (provider == null)
            {
                return TypeValidationResult.Error("IValueProvider 為 null");
            }

            // 當需要變數驗證時，檢查是否為 IVariableProvider 且變數有效
            if (Attribute.IsVariableNeeded)
            {
                if (provider is not IVariableProvider variableProvider)
                    return TypeValidationResult.Error("並非 IVariableProvider");
                if (!variableProvider.IsVariableValid)
                    return TypeValidationResult.Error("IValueProvider 的變數無效或未設置");

                // 根據期望型別決定驗證策略
                var actualType = ShouldValidateVariableType()
                    ? variableProvider.VariableType
                    : provider.ValueType;
                if (actualType == null)
                {
                    var typeDescription = ShouldValidateVariableType()
                        ? "VariableType"
                        : "ValueType";
                    return TypeValidationResult.Error($"IValueProvider.{typeDescription} 為 null");
                }

                return ValidateTypeCompatibility(actualType);
            }
            else
            {
                // 驗證值型別（如 float, int 等）
                var actualValueType = provider.ValueType;
                if (actualValueType == null)
                {
                    return TypeValidationResult.Error("IValueProvider.ValueType 為 null");
                }

                return ValidateTypeCompatibility(actualValueType);
            }
        }

        /// <summary>
        /// 判斷是否應該驗證變數型別而非值型別
        /// </summary>
        private bool ShouldValidateVariableType()
        {
            // 如果不需要變數驗證，直接返回 false
            if (!Attribute.IsVariableNeeded)
                return false;

            // 檢查期望型別是否為變數型別（繼承自 AbstractMonoVariable）
            if (Attribute.ExpectedType != null)
            {
                return typeof(AbstractMonoVariable).IsAssignableFrom(Attribute.ExpectedType);
            }

            // 檢查多型別模式下是否有變數型別
            if (Attribute.ExpectedTypes != null)
            {
                return Attribute.ExpectedTypes.Any(type =>
                    typeof(AbstractMonoVariable).IsAssignableFrom(type)
                );
            }

            // 如果無法判斷，預設為驗證值型別
            return false;
        }

        /// <summary>
        /// 驗證型別相容性（統一處理單一型別和多型別模式）
        /// </summary>
        private TypeValidationResult ValidateTypeCompatibility(Type actualType)
        {
            // 檢查是否使用多型別模式
            if (IsMultiTypeMode())
                return ValidateMultiType(actualType);
            else
                return ValidateSingleType(actualType);
        }

        /// <summary>
        /// 檢查是否使用多型別模式
        /// </summary>
        private bool IsMultiTypeMode()
        {
            return Attribute.ExpectedTypes != null
                || !string.IsNullOrEmpty(Attribute.DynamicTypePropertyName);
        }

        /// <summary>
        /// 驗證單一型別模式
        /// </summary>
        private TypeValidationResult ValidateSingleType(Type actualType)
        {
            var expectedType = Attribute.ExpectedType;

            if (actualType == expectedType)
                return TypeValidationResult.Success();

            var compatibilityResult = CheckTypeCompatibility(actualType, expectedType);
            if (compatibilityResult != null)
                return compatibilityResult;

            return CreateIncompatibleTypeError(actualType, new[] { expectedType });
        }

        /// <summary>
        /// 驗證多型別模式
        /// </summary>
        private TypeValidationResult ValidateMultiType(Type actualType)
        {
            var target = Property.ParentValues.FirstOrDefault();
            if (target == null)
                return TypeValidationResult.Error("無法取得驗證目標物件");

            var expectedTypes = Attribute.GetExpectedTypes(target).ToList();
            if (!expectedTypes.Any() && !Attribute.IsVariableNeeded)
                return TypeValidationResult.Error("沒有指定任何期望的型別");

            if (expectedTypes.Contains(actualType))
                return TypeValidationResult.Success();

            var bestMatch = FindBestCompatibleType(actualType, expectedTypes);
            var compatibilityResult =
                bestMatch != null ? CheckTypeCompatibility(actualType, bestMatch) : null;
            if (compatibilityResult != null)
                return compatibilityResult;

            if (expectedTypes.Count == 0 && Attribute.IsVariableNeeded)
                return TypeValidationResult.Success();

            return CreateIncompatibleTypeError(actualType, expectedTypes);
        }

        /// <summary>
        /// 檢查型別相容性
        /// </summary>
        private TypeValidationResult CheckTypeCompatibility(Type actualType, Type expectedType)
        {
            if (
                !Attribute.AllowCompatibleTypes
                || !TypeCompatibilityChecker.AreCompatible(actualType, expectedType)
            )
                return null;

            var score = TypeCompatibilityChecker.GetCompatibilityScore(actualType, expectedType);
            if (score >= 80)
                return TypeValidationResult.Success();
            if (score >= 60)
                return TypeValidationResult.Warning(
                    $"型別相容但可能有精度損失：{actualType.Name} → {expectedType.Name}",
                    expectedType,
                    actualType
                );

            return null;
        }

        /// <summary>
        /// 建立型別不相容錯誤
        /// </summary>
        private TypeValidationResult CreateIncompatibleTypeError(
            Type actualType,
            IEnumerable<Type> expectedTypes
        )
        {
            var typesList = expectedTypes.ToList();
            if (!typesList.Any())
                return TypeValidationResult.Error(
                    "沒有指定任何期望的型別，無法進行驗證。請檢查 ValueTypeValidateAttribute 的配置。"
                );

            var expectedTypeNames = string.Join("、", typesList.Select(t => t.Name));
            var errorMessage = !string.IsNullOrEmpty(Attribute.CustomErrorMessage)
                ? Attribute.CustomErrorMessage
                : $"ValueType 不符合期望。期望：{expectedTypeNames}，實際：{actualType.Name}";

            var result = TypeValidationResult.Error(errorMessage, typesList.First(), actualType);
            result.Suggestions = GenerateSuggestionsForTypes(actualType, typesList);

            return result;
        }

        /// <summary>
        /// 為多個型別產生建議
        /// </summary>
        private List<string> GenerateSuggestionsForTypes(
            Type actualType,
            IEnumerable<Type> expectedTypes
        )
        {
            var allSuggestions = new List<string>();
            foreach (var expectedType in expectedTypes)
            {
                var suggestions = TypeCompatibilityChecker.GetConversionSuggestions(
                    actualType,
                    expectedType
                );
                if (suggestions != null)
                    allSuggestions.AddRange(suggestions);
            }
            return allSuggestions.Distinct().ToList();
        }

        /// <summary>
        /// 從屬性獲取期望的型別
        /// </summary>
        // private IEnumerable<Type> GetExpectedTypesFromAttribute(object target)
        // {
        //     var result = new List<Type>();
        //
        //     // 靜態型別陣列模式
        //     if (Attribute.ExpectedTypes != null) result.AddRange(Attribute.ExpectedTypes);
        //
        //     // 動態型別模式
        //     if (!string.IsNullOrEmpty(Attribute.DynamicTypePropertyName) && target != null)
        //     {
        //         // 首先嘗試標準的方法查找（public 方法）
        //         var method = target.GetType().GetProperty(Attribute.DynamicTypePropertyName,
        //             System.Reflection.BindingFlags.Instance |
        //             System.Reflection.BindingFlags.Public);
        //
        //         // 如果找不到，使用 ReflectionHelperMethods 來查找繼承階層中的非公開方法
        //         if (method == null)
        //             method = ReflectionHelperMethods.GetNonPublicMethodInBaseClasses(target.GetType(),
        //                 Attribute.DynamicTypePropertyName);
        //
        //         if (method != null)
        //             try
        //             {
        //                 var methodResult = method.Invoke(target, null);
        //                 if (methodResult is IEnumerable<Type> types)
        //                     result.AddRange(types);
        //                 else if (methodResult is Type singleType) result.Add(singleType);
        //             }
        //             catch (Exception ex)
        //             {
        //                 Debug.LogError($"執行動態型別方法 '{Attribute.DynamicTypePropertyName}' 時發生錯誤: {ex.Message}");
        //             }
        //         else
        //             Debug.LogError($"找不到方法 '{Attribute.DynamicTypePropertyName}' 在型別 {target.GetType().Name} 中");
        //     }
        //
        //     return result;
        // }

        /// <summary>
        /// 在期望的型別中找到與實際型別最相容的型別
        /// </summary>
        private Type FindBestCompatibleType(Type actualType, IEnumerable<Type> expectedTypes)
        {
            Type bestMatch = null;
            var bestScore = 0;

            foreach (var expectedType in expectedTypes)
                if (TypeCompatibilityChecker.AreCompatible(actualType, expectedType))
                {
                    var score = TypeCompatibilityChecker.GetCompatibilityScore(
                        actualType,
                        expectedType
                    );
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMatch = expectedType;
                    }
                }

            return bestMatch;
        }
    }
}

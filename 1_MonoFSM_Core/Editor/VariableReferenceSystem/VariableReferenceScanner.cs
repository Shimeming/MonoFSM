using System;
using System.Collections.Generic;
using System.Reflection;
using MonoFSM.Core.DataProvider;
using MonoFSM.Runtime;
using MonoFSM.Variable;
using UnityEngine;

namespace MonoFSM.Editor.VariableReferenceSystem
{
    /// <summary>
    /// 掃描並建立 Variable 引用關係的核心掃描器
    /// </summary>
    public static class VariableReferenceScanner
    {
        // 快取：Variable -> 引用該 Variable 的資訊列表
        private static Dictionary<AbstractMonoVariable, List<VariableReferenceInfo>> _referenceCache = new();

        // 快取的根物件
        private static GameObject _cachedRoot;

        /// <summary>
        /// 取得快取的根物件
        /// </summary>
        public static GameObject CachedRoot => _cachedRoot;

        /// <summary>
        /// 檢查快取是否有效
        /// </summary>
        public static bool HasValidCache => _cachedRoot != null && _referenceCache.Count > 0;

        /// <summary>
        /// 清除快取
        /// </summary>
        public static void ClearCache()
        {
            _referenceCache.Clear();
            _cachedRoot = null;
        }

        /// <summary>
        /// 從指定根物件掃描所有 Variable 引用
        /// </summary>
        public static void ScanFromRoot(GameObject root)
        {
            ClearCache();
            if (root == null) return;

            _cachedRoot = root;
            var components = root.GetComponentsInChildren<Component>(true);

            foreach (var comp in components)
            {
                if (comp == null) continue;
                ScanComponent(comp);
            }
        }

        /// <summary>
        /// 取得指定 Variable 的所有引用資訊
        /// </summary>
        public static List<VariableReferenceInfo> GetReferences(AbstractMonoVariable variable)
        {
            if (variable == null)
                return new List<VariableReferenceInfo>();

            if (_referenceCache.TryGetValue(variable, out var list))
                return list;

            return new List<VariableReferenceInfo>();
        }

        /// <summary>
        /// 取得所有被引用的 Variable 列表
        /// </summary>
        public static IEnumerable<AbstractMonoVariable> GetAllReferencedVariables()
        {
            return _referenceCache.Keys;
        }

        /// <summary>
        /// 掃描單一 Component
        /// </summary>
        private static void ScanComponent(Component comp)
        {
            var compType = comp.GetType();
            var fields = compType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                // 1. 檢查直接引用 (欄位類型繼承 AbstractMonoVariable)
                if (typeof(AbstractMonoVariable).IsAssignableFrom(field.FieldType))
                {
                    ScanDirectField(comp, field);
                }
                // 2. 檢查 VarWrapper 引用
                else if (typeof(AbstractVarWrapper).IsAssignableFrom(field.FieldType))
                {
                    ScanVarWrapperField(comp, field);
                }
                // 3. 檢查 ValueProvider 引用
                else if (typeof(ValueProvider).IsAssignableFrom(field.FieldType))
                {
                    ScanValueProviderField(comp, field);
                }
            }
        }

        /// <summary>
        /// 掃描直接欄位引用
        /// </summary>
        private static void ScanDirectField(Component comp, FieldInfo field)
        {
            var variable = field.GetValue(comp) as AbstractMonoVariable;
            if (variable == null) return;

            // 跳過自己引用自己的情況
            if (ReferenceEquals(variable, comp)) return;

            var info = CreateReferenceInfo(
                variable,
                comp,
                field.Name,
                ReferenceType.DirectField
            );

            AddToCache(variable, info);
        }

        /// <summary>
        /// 掃描 VarWrapper 欄位引用
        /// </summary>
        private static void ScanVarWrapperField(Component comp, FieldInfo wrapperField)
        {
            var wrapper = wrapperField.GetValue(comp);
            if (wrapper == null) return;

            // 找到 _var 欄位
            var wrapperType = wrapper.GetType();
            var varField = FindFieldInHierarchy(wrapperType, "_var");

            if (varField == null) return;

            var variable = varField.GetValue(wrapper) as AbstractMonoVariable;
            if (variable == null) return;

            var info = CreateReferenceInfo(
                variable,
                comp,
                $"{wrapperField.Name}._var",
                ReferenceType.VarWrapper
            );

            AddToCache(variable, info);
        }

        /// <summary>
        /// 掃描 ValueProvider 欄位引用
        /// </summary>
        private static void ScanValueProviderField(Component comp, FieldInfo providerField)
        {
            var provider = providerField.GetValue(comp) as ValueProvider;
            if (provider == null) return;

            // ValueProvider 透過 _varEntity + _varTag 來引用
            // 先嘗試取得 VarRaw
            var varRaw = provider.VarRaw;
            if (varRaw == null) return;

            var info = CreateReferenceInfo(
                varRaw,
                comp,
                $"{providerField.Name} (ValueProvider)",
                ReferenceType.ValueProvider
            );

            AddToCache(varRaw, info);
        }

        /// <summary>
        /// 在類型繼承鏈中尋找欄位
        /// </summary>
        private static FieldInfo FindFieldInHierarchy(Type type, string fieldName)
        {
            while (type != null)
            {
                var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null)
                    return field;
                type = type.BaseType;
            }
            return null;
        }

        /// <summary>
        /// 建立引用資訊
        /// </summary>
        private static VariableReferenceInfo CreateReferenceInfo(
            AbstractMonoVariable variable,
            Component referencingComponent,
            string fieldPath,
            ReferenceType type)
        {
            var ownerEntity = referencingComponent.GetComponentInParent<MonoEntity>();
            var varEntity = variable.GetComponentInParent<MonoEntity>();

            var scope = (ownerEntity == varEntity && ownerEntity != null)
                ? ReferenceScope.Local
                : ReferenceScope.CrossEntity;

            return new VariableReferenceInfo
            {
                TargetVariable = variable,
                ReferencingComponent = referencingComponent,
                FieldPath = fieldPath,
                Type = type,
                Scope = scope,
                OwnerEntity = ownerEntity
            };
        }

        /// <summary>
        /// 加入快取
        /// </summary>
        private static void AddToCache(AbstractMonoVariable variable, VariableReferenceInfo info)
        {
            if (!_referenceCache.TryGetValue(variable, out var list))
            {
                list = new List<VariableReferenceInfo>();
                _referenceCache[variable] = list;
            }
            list.Add(info);
        }
    }
}

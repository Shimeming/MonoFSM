/* Author: Jerry
 * Summary: This attribute automatically processes nested class fields that contain Auto attributes.
 * It recursively finds and executes Auto attributes in nested objects/classes.
 *  * Usage example:
 *
 * public class MyBehaviour : MonoBehaviour
 * {
 *     [AutoNested] public ConditionGroup conditionGroup;  // This will process Auto attributes inside ConditionGroup
 * }
 *
 * public class ConditionGroup
 * {
 *     [AutoChildren] private AbstractConditionBehaviour[] conditions; // This will be processed by AutoNested
 * }
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Auto.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

[IncludeMyAttributes]
[AttributeUsage(AttributeTargets.Field)]
public class AutoNestedAttribute : AbstractAutoAttribute
{
    private readonly int _maxDepth;

    public AutoNestedAttribute(bool logMissingAsError = true, int maxDepth = 3)
        : base(logMissingAsError)
    {
        _maxDepth = maxDepth;
    }

    public override bool Execute(MonoBehaviour mb, FieldInfo field)
    {
        var fieldValue = field.GetValue(mb);
        if (fieldValue == null)
        {
            Debug.LogWarning(
                $"[AutoNested] Field {field.Name} is null in {mb.name}, cannot process nested auto attributes.",
                mb
            );
            return false;
        }

        // Debug.Log("[AutoNested] Processing nested auto attributes in field " + field.Name, mb);

        return ProcessNestedObject(mb, fieldValue, field.FieldType, 0, field);
    }

    private bool ProcessNestedObject(
        MonoBehaviour rootMb,
        object obj,
        Type objType,
        int currentDepth,
        FieldInfo parentField
    )
    {
        if (obj == null || currentDepth >= _maxDepth)
            return false;

        var allSuccess = true;
        var nestedFields = GetFieldsWithAuto(objType);

        foreach (var nestedField in nestedFields)
        {
            var attributes = nestedField.GetCustomAttributes(typeof(IAutoAttribute), true);

            foreach (IAutoAttribute autoAttribute in attributes)
            {
                // 創建一個包裝的 FieldInfo，讓 Auto attribute 能正確處理嵌套對象
                var wrappedField = new NestedFieldWrapper(nestedField, obj);
                var result = autoAttribute.Execute(rootMb, wrappedField);
                if (!result)
                    allSuccess = false;
            }

            // 如果這個 field 本身也可能包含嵌套的 Auto attributes，遞迴處理
            var nestedValue = nestedField.GetValue(obj);
            if (
                nestedValue != null
                && !nestedField.FieldType.IsPrimitive
                && !nestedField.FieldType.IsEnum
                && nestedField.FieldType != typeof(string)
            )
                ProcessNestedObject(
                    rootMb,
                    nestedValue,
                    nestedField.FieldType,
                    currentDepth + 1,
                    nestedField
                );
        }

        return allSuccess;
    }

    private IEnumerable<FieldInfo> GetFieldsWithAuto(Type type)
    {
        var fields = type.GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            )
            .Where(field => !field.FieldType.IsPrimitive)
            .Where(field =>
                IsDefined(field, typeof(AutoAttribute))
                || IsDefined(field, typeof(AutoChildrenAttribute))
                || IsDefined(field, typeof(AutoParentAttribute))
            );

        // 也包含基類的 non-public fields
        var baseClassFields = ReflectionHelperMethods
            .GetNonPublicFieldsInBaseClasses(type)
            .Where(field =>
                IsDefined(field, typeof(AutoAttribute))
                || IsDefined(field, typeof(AutoChildrenAttribute))
                || IsDefined(field, typeof(AutoParentAttribute))
            );

        return fields.Concat(baseClassFields);
    }
}

/// <summary>
///     包裝器類別，讓 Auto attributes 能正確處理嵌套對象的 fields
/// </summary>
public class NestedFieldWrapper : FieldInfo
{
    private readonly FieldInfo _nestedField;
    private readonly object _nestedObject;

    public NestedFieldWrapper(FieldInfo nestedField, object nestedObject)
    {
        _nestedField = nestedField;
        _nestedObject = nestedObject;
    }

    public override object GetValue(object obj)
    {
        // 從嵌套對象中取值
        return _nestedField.GetValue(_nestedObject);
    }

    public override void SetValue(
        object obj,
        object value,
        BindingFlags invokeAttr,
        Binder binder,
        CultureInfo culture
    )
    {
        // 設定值到嵌套對象中
        _nestedField.SetValue(_nestedObject, value);
    }

    // 委派所有其他屬性到嵌套的 field
    public override string Name => _nestedField.Name;
    public override Type FieldType => _nestedField.FieldType;
    public override Type DeclaringType => _nestedField.DeclaringType;
    public override RuntimeFieldHandle FieldHandle => _nestedField.FieldHandle;
    public override FieldAttributes Attributes => _nestedField.Attributes;
    public override Type ReflectedType => _nestedField.ReflectedType;

    public override object[] GetCustomAttributes(bool inherit)
    {
        return _nestedField.GetCustomAttributes(inherit);
    }

    public override object[] GetCustomAttributes(Type attributeType, bool inherit)
    {
        return _nestedField.GetCustomAttributes(attributeType, inherit);
    }

    public override bool IsDefined(Type attributeType, bool inherit)
    {
        return _nestedField.IsDefined(attributeType, inherit);
    }
}

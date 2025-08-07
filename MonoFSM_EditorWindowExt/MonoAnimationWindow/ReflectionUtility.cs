#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using Type = System.Type;


namespace MonoFSMEditor
{
    public static class RefectionUtility
    {

        #region Reflection


        public static object GetFieldValue(this object o, string fieldName)
        {
            var type = o as Type ?? o.GetType();
            var target = o is Type ? null : o;


            if (type.GetFieldInfo(fieldName) is FieldInfo fieldInfo)
                return fieldInfo.GetValue(target);


            throw new System.Exception($"Field '{fieldName}' not found in type '{type.Name}' and its parent types");

        }

        public static object GetPropertyValue(this object o, string propertyName)
        {
            var type = o as Type ?? o.GetType();
            var target = o is Type ? null : o;


            if (type.GetPropertyInfo(propertyName) is PropertyInfo propertyInfo)
                return propertyInfo.GetValue(target);


            throw new System.Exception(
                $"Property '{propertyName}' not found in type '{type.Name}' and its parent types");

        }

        public static object GetMemberValue(this object o, string memberName)
        {
            var type = o as Type ?? o.GetType();
            var target = o is Type ? null : o;


            if (type.GetFieldInfo(memberName) is FieldInfo fieldInfo)
                return fieldInfo.GetValue(target);

            if (type.GetPropertyInfo(memberName) is PropertyInfo propertyInfo)
                return propertyInfo.GetValue(target);


            throw new System.Exception($"Member '{memberName}' not found in type '{type.Name}' and its parent types");

        }

        public static void SetFieldValue(this object o, string fieldName, object value)
        {
            var type = o as Type ?? o.GetType();
            var target = o is Type ? null : o;


            if (type.GetFieldInfo(fieldName) is FieldInfo fieldInfo)
                fieldInfo.SetValue(target, value);


            else
                throw new System.Exception($"Field '{fieldName}' not found in type '{type.Name}' and its parent types");

        }

        public static void SetPropertyValue(this object o, string propertyName, object value)
        {
            var type = o as Type ?? o.GetType();
            var target = o is Type ? null : o;


            if (type.GetPropertyInfo(propertyName) is PropertyInfo propertyInfo)
                propertyInfo.SetValue(target, value);


            else
                throw new System.Exception(
                    $"Property '{propertyName}' not found in type '{type.Name}' and its parent types");

        }

        public static void SetMemberValue(this object o, string memberName, object value)
        {
            var type = o as Type ?? o.GetType();
            var target = o is Type ? null : o;


            if (type.GetFieldInfo(memberName) is FieldInfo fieldInfo)
                fieldInfo.SetValue(target, value);

            else if (type.GetPropertyInfo(memberName) is PropertyInfo propertyInfo)
                propertyInfo.SetValue(target, value);


            else
                throw new System.Exception(
                    $"Member '{memberName}' not found in type '{type.Name}' and its parent types");

        }

        public static object
            InvokeMethod(this object o, string methodName,
                params object[] parameters) // todo handle null params (can't get their type)
        {
            var type = o as Type ?? o.GetType();
            var target = o is Type ? null : o;


            if (type.GetMethodInfo(methodName, parameters.Select(r => r.GetType()).ToArray()) is MethodInfo methodInfo)
                return methodInfo.Invoke(target, parameters);


            throw new System.Exception(
                $"Method '{methodName}' not found in type '{type.Name}', its parent types and interfaces");

        }


        public static T GetFieldValue<T>(this object o, string fieldName) => (T)o.GetFieldValue(fieldName);
        public static T GetPropertyValue<T>(this object o, string propertyName) => (T)o.GetPropertyValue(propertyName);
        public static T GetMemberValue<T>(this object o, string memberName) => (T)o.GetMemberValue(memberName);

        public static T InvokeMethod<T>(this object o, string methodName, params object[] parameters) =>
            (T)o.InvokeMethod(methodName, parameters);




        public static FieldInfo GetFieldInfo(this Type type, string fieldName)
        {
            if (fieldInfoCache.TryGetValue(type, out var fieldInfosByNames))
                if (fieldInfosByNames.TryGetValue(fieldName, out var fieldInfo))
                    return fieldInfo;


            if (!fieldInfoCache.ContainsKey(type))
                fieldInfoCache[type] = new Dictionary<string, FieldInfo>();

            for (var curType = type; curType != null; curType = curType.BaseType)
                if (curType.GetField(fieldName, maxBindingFlags) is FieldInfo fieldInfo)
                    return fieldInfoCache[type][fieldName] = fieldInfo;


            return fieldInfoCache[type][fieldName] = null;

        }

        public static PropertyInfo GetPropertyInfo(this Type type, string propertyName)
        {
            if (propertyInfoCache.TryGetValue(type, out var propertyInfosByNames))
                if (propertyInfosByNames.TryGetValue(propertyName, out var propertyInfo))
                    return propertyInfo;


            if (!propertyInfoCache.ContainsKey(type))
                propertyInfoCache[type] = new Dictionary<string, PropertyInfo>();

            for (var curType = type; curType != null; curType = curType.BaseType)
                if (curType.GetProperty(propertyName, maxBindingFlags) is PropertyInfo propertyInfo)
                    return propertyInfoCache[type][propertyName] = propertyInfo;


            return propertyInfoCache[type][propertyName] = null;

        }

        public static MethodInfo GetMethodInfo(this Type type, string methodName, params Type[] argumentTypes)
        {
            var methodHash = methodName.GetHashCode() ^
                             argumentTypes.Aggregate(0, (hash, r) => hash ^= r.GetHashCode());


            if (methodInfoCache.TryGetValue(type, out var methodInfosByHashes))
                if (methodInfosByHashes.TryGetValue(methodHash, out var methodInfo))
                    return methodInfo;



            if (!methodInfoCache.ContainsKey(type))
                methodInfoCache[type] = new Dictionary<int, MethodInfo>();

            for (var curType = type; curType != null; curType = curType.BaseType)
                if (curType.GetMethod(methodName, maxBindingFlags, null, argumentTypes, null) is MethodInfo methodInfo)
                    return methodInfoCache[type][methodHash] = methodInfo;

            foreach (var interfaceType in type.GetInterfaces())
                if (interfaceType.GetMethod(methodName, maxBindingFlags, null, argumentTypes, null) is MethodInfo
                    methodInfo)
                    return methodInfoCache[type][methodHash] = methodInfo;



            return methodInfoCache[type][methodHash] = null;

        }

        static Dictionary<Type, Dictionary<string, FieldInfo>> fieldInfoCache = new();
        static Dictionary<Type, Dictionary<string, PropertyInfo>> propertyInfoCache = new();
        static Dictionary<Type, Dictionary<int, MethodInfo>> methodInfoCache = new();






        public static T GetCustomAttributeCached<T>(this MemberInfo memberInfo) where T : System.Attribute
        {
            if (!attributesCache.TryGetValue(memberInfo, out var attributes_byType))
                attributes_byType = attributesCache[memberInfo] = new();

            if (!attributes_byType.TryGetValue(typeof(T), out var attribute))
                attribute = attributes_byType[typeof(T)] = memberInfo.GetCustomAttribute<T>();

            return attribute as T;

        }

        static Dictionary<MemberInfo, Dictionary<Type, System.Attribute>> attributesCache = new();






        public static List<Type> GetSubclasses(this Type t) =>
            t.Assembly.GetTypes().Where(type => type.IsSubclassOf(t)).ToList();

        public static object GetDefaultValue(this FieldInfo f, params object[] constructorVars) =>
            f.GetValue(System.Activator.CreateInstance(((MemberInfo)f).ReflectedType, constructorVars));

        public static object GetDefaultValue(this FieldInfo f) =>
            f.GetValue(System.Activator.CreateInstance(((MemberInfo)f).ReflectedType));

        public static IEnumerable<FieldInfo> GetFieldsWithoutBase(this Type t) =>
            t.GetFields().Where(r => !t.BaseType.GetFields().Any(rr => rr.Name == r.Name));

        public static IEnumerable<PropertyInfo> GetPropertiesWithoutBase(this Type t) => t.GetProperties()
            .Where(r => !t.BaseType.GetProperties().Any(rr => rr.Name == r.Name));


        public const BindingFlags maxBindingFlags = (BindingFlags)62;








        #endregion

        #region Rects


        public static Rect Resize(this Rect rect, float px)
        {
            rect.x += px;
            rect.y += px;
            rect.width -= px * 2;
            rect.height -= px * 2;
            return rect;
        }

        public static Rect SetPos(this Rect rect, Vector2 v) => rect.SetPos(v.x, v.y);

        public static Rect SetPos(this Rect rect, float x, float y)
        {
            rect.x = x;
            rect.y = y;
            return rect;
        }

        public static Rect SetX(this Rect rect, float x) => rect.SetPos(x, rect.y);
        public static Rect SetY(this Rect rect, float y) => rect.SetPos(rect.x, y);

        public static Rect SetXMax(this Rect rect, float xMax)
        {
            rect.xMax = xMax;
            return rect;
        }

        public static Rect SetYMax(this Rect rect, float yMax)
        {
            rect.yMax = yMax;
            return rect;
        }

        public static Rect SetMidPos(this Rect r, Vector2 v) => r.SetPos(v).MoveX(-r.width / 2).MoveY(-r.height / 2);
        public static Rect SetMidPos(this Rect r, float x, float y) => r.SetMidPos(new Vector2(x, y));

        public static Rect Move(this Rect rect, Vector2 v)
        {
            rect.position += v;
            return rect;
        }

        public static Rect Move(this Rect rect, float x, float y)
        {
            rect.x += x;
            rect.y += y;
            return rect;
        }

        public static Rect MoveX(this Rect rect, float px)
        {
            rect.x += px;
            return rect;
        }

        public static Rect MoveY(this Rect rect, float px)
        {
            rect.y += px;
            return rect;
        }

        public static Rect SetWidth(this Rect rect, float f)
        {
            rect.width = f;
            return rect;
        }

        public static Rect SetWidthFromMid(this Rect rect, float px)
        {
            rect.x += rect.width / 2;
            rect.width = px;
            rect.x -= rect.width / 2;
            return rect;
        }

        public static Rect SetWidthFromRight(this Rect rect, float px)
        {
            rect.x += rect.width;
            rect.width = px;
            rect.x -= rect.width;
            return rect;
        }

        public static Rect SetHeight(this Rect rect, float f)
        {
            rect.height = f;
            return rect;
        }

        public static Rect SetHeightFromMid(this Rect rect, float px)
        {
            rect.y += rect.height / 2;
            rect.height = px;
            rect.y -= rect.height / 2;
            return rect;
        }

        public static Rect SetHeightFromBottom(this Rect rect, float px)
        {
            rect.y += rect.height;
            rect.height = px;
            rect.y -= rect.height;
            return rect;
        }

        public static Rect AddWidth(this Rect rect, float f) => rect.SetWidth(rect.width + f);
        public static Rect AddWidthFromMid(this Rect rect, float f) => rect.SetWidthFromMid(rect.width + f);
        public static Rect AddWidthFromRight(this Rect rect, float f) => rect.SetWidthFromRight(rect.width + f);

        public static Rect AddHeight(this Rect rect, float f) => rect.SetHeight(rect.height + f);
        public static Rect AddHeightFromMid(this Rect rect, float f) => rect.SetHeightFromMid(rect.height + f);
        public static Rect AddHeightFromBottom(this Rect rect, float f) => rect.SetHeightFromBottom(rect.height + f);

        public static Rect SetSize(this Rect rect, Vector2 v) => rect.SetWidth(v.x).SetHeight(v.y);
        public static Rect SetSize(this Rect rect, float w, float h) => rect.SetWidth(w).SetHeight(h);

        public static Rect SetSize(this Rect rect, float f)
        {
            rect.height = rect.width = f;
            return rect;
        }

        public static Rect SetSizeFromMid(this Rect r, Vector2 v) => r.Move(r.size / 2).SetSize(v).Move(-v / 2);
        public static Rect SetSizeFromMid(this Rect r, float x, float y) => r.SetSizeFromMid(new Vector2(x, y));
        public static Rect SetSizeFromMid(this Rect r, float f) => r.SetSizeFromMid(new Vector2(f, f));

        public static Rect AlignToPixelGrid(this Rect r) => GUIUtility.AlignRectToDevice(r);





        #endregion
    }
}
#endif
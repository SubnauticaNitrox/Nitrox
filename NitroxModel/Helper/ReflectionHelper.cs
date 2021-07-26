using System;
using System.Linq;
using System.Reflection;

namespace NitroxModel.Helper
{
    public static class ReflectionHelper
    {
        // Public calls are useful for reflected, inaccessible objects.
        public static object ReflectionCall<T>(this T o, string methodName, bool isPublic = false, bool isStatic = false, params object[] args)
        {
            ValidateStatic(o, isStatic);
            Type t = isStatic ? typeof(T) : o.GetType();
            MethodInfo methodInfo = GetMethod(t, methodName, isPublic, isStatic);
            return methodInfo.Invoke(o, args);
        }

        public static object ReflectionCall<T>(this T o, string methodName, Type[] types, bool isPublic = false, bool isStatic = false, params object[] args)
        {
            ValidateStatic(o, isStatic);
            Type t = isStatic ? typeof(T) : o.GetType();
            MethodInfo methodInfo = GetMethod(t, methodName, isPublic, isStatic, types);
            return methodInfo.Invoke(o, args);
        }

        public static object ReflectionGet<T>(this T o, string fieldName, bool isPublic = false, bool isStatic = false)
        {
            ValidateStatic(o, isStatic);
            Type t = isStatic ? typeof(T) : o.GetType();
            FieldInfo fieldInfo = GetField(t, fieldName, isPublic, isStatic);
            return fieldInfo.GetValue(o);
        }

        public static object ReflectionGet(this object o, FieldInfo fieldInfo)
        {
            Validate.NotNull(fieldInfo, $"Field cannot be null!");
            return fieldInfo.GetValue(o);
        }

        public static object ReflectionGet<T, T2>(this T2 o, string fieldName, bool isPublic = false, bool isStatic = false)
            where T2 : T
        {
            ValidateStatic(o, isStatic);
            Type t = typeof(T);
            FieldInfo fieldInfo = GetField(t, fieldName, isPublic, isStatic);
            return fieldInfo.GetValue(o);
        }

        public static object ReflectionGetProperty<T>(this T o, string fieldName)
        {
            Type t = typeof(T);
            PropertyInfo propertyInfo = GetProperty(t, fieldName);
            return propertyInfo.GetValue(o);
        }

        public static void ReflectionSet<T>(this T o, string fieldName, object value, bool isPublic = false, bool isStatic = false)
        {
            ValidateStatic(o, isStatic);
            Type t = isStatic ? typeof(T) : o.GetType();
            FieldInfo fieldInfo = GetField(t, fieldName, isPublic, isStatic);
            fieldInfo.SetValue(o, value);
        }

        public static void ReflectionSet(this object o, FieldInfo fieldInfo, object value)
        {
            Validate.NotNull(fieldInfo, $"Field cannot be null!");
            fieldInfo.SetValue(o, value);
        }

        public static void ReflectionSet<T, T2>(this T2 o, string fieldName, object value, bool isPublic = false, bool isStatic = false)
            where T2 : T
        {
            ValidateStatic(o, isStatic);
            Type t = typeof(T);
            FieldInfo fieldInfo = GetField(t, fieldName, isPublic, isStatic);
            fieldInfo.SetValue(o, value);
        }

        public static MethodInfo GetMethod<T>(string methodName, bool isPublic = false, bool isStatic = false, params Type[] types)
        {
            return GetMethod(typeof(T), methodName, isPublic, isStatic, types);
        }

        private static MethodInfo GetMethod(this Type t, string methodName, bool isPublic = false, bool isStatic = false, params Type[] types)
        {
            MethodInfo methodInfo;
            BindingFlags bindingFlags = GetBindingFlagsFromMethodQualifiers(isPublic, isStatic);
            if (types != null && types.Length > 0)
            {
                methodInfo = t.GetMethod(methodName, bindingFlags, null, types, null);
                Validate.NotNull(methodInfo, $"Type \"{t.Name}\" does not have a method called \"{methodName}\", with bindingFlags {bindingFlags} and types {string.Join(", ", types.Select(typ => typ.ToString()).ToArray())}.");
            }
            else
            {
                methodInfo = t.GetMethod(methodName, bindingFlags);
                Validate.NotNull(methodInfo, $"Type \"{t.Name}\" does not have a method called \"{methodName}\" with bindingFlags {bindingFlags}.");
            }

            return methodInfo;
        }

        public static FieldInfo GetField<T>(string fieldName, bool isPublic = false, bool isStatic = false)
        {
            return GetField(typeof(T), fieldName, isPublic, isStatic);
        }

        private static FieldInfo GetField(this Type t, string fieldName, bool isPublic = false, bool isStatic = false)
        {
            BindingFlags bindingFlags = GetBindingFlagsFromMethodQualifiers(isPublic, isStatic);
            FieldInfo fieldInfo = t.GetField(fieldName, bindingFlags);
            Validate.NotNull(fieldInfo, $"Type \"{t.Name}\" does not have a field called \"{fieldName}\" with bindingFlags {bindingFlags}.");
            return fieldInfo;
        }

        private static PropertyInfo GetProperty(this Type t, string propertyName)
        {
            BindingFlags bindingFlags = GetBindingFlagsFromMethodQualifiers(false, false);
            PropertyInfo propertyInfo = t.GetProperty(propertyName, bindingFlags);
            Validate.NotNull(propertyInfo, $"Type \"{t.Name}\" does not have a property called \"{propertyName}\" with bindingFlags {bindingFlags}.");

            return propertyInfo;
        }

        private static BindingFlags GetBindingFlagsFromMethodQualifiers(bool isPublic, bool isStatic)
        {
            BindingFlags bindingFlags = isPublic ? BindingFlags.Public : BindingFlags.NonPublic;
            bindingFlags |= isStatic ? BindingFlags.Static : BindingFlags.Instance;

            return bindingFlags;
        }

        private static void ValidateStatic(object o, bool isStatic)
        {
            if (o == null && !isStatic)
            {
                throw new ArgumentException("Object can't be null when isStatic is false!");
            }
            else if (o != null && isStatic)
            {
                throw new ArgumentException("Object must be be null when isStatic is true!");
            }
        }
    }
}

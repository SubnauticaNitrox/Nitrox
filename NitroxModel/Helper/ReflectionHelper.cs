using System;
using System.Reflection;

namespace NitroxModel.Helper
{
    public static class ReflectionHelper
    {
        // Public calls are useful for reflected, inaccessible objects.
        // TODO log bindingFlags as well in Validate.NotNull calls.
        public static object ReflectionCall<T>(this T o, string methodName, bool isPublic = false, bool isStatic = false, params object[] args)
        {
            Validate.IsTrue(o != null ^ isStatic);
            Type t = isStatic ? typeof(T) : o.GetType();
            BindingFlags bindingFlags = GetBindingFlagsFromMethodQualifiers(isPublic, isStatic);
            MethodInfo methodInfo = t.GetMethod(methodName, bindingFlags);
            Validate.NotNull(methodInfo, $"Class \"{t.Name}\" does not have a method called \"{methodName}\".");
            return methodInfo.Invoke(o, args);
        }

        public static object ReflectionCall<T>(this T o, string methodName, Type[] types, bool isPublic = false, bool isStatic = false, params object[] args)
        {
            Validate.IsTrue(o != null ^ isStatic);
            Type t = isStatic ? typeof(T) : o.GetType();
            BindingFlags bindingFlags = GetBindingFlagsFromMethodQualifiers(isPublic, isStatic);
            MethodInfo methodInfo = t.GetMethod(methodName, bindingFlags, null, types, null);
            Validate.NotNull(methodInfo, $"Class \"{t.Name}\" does not have a method called \"{methodName}\".");
            return methodInfo.Invoke(o, args);
        }

        public static object ReflectionGet<T>(this T o, string fieldName, bool isPublic = false, bool isStatic = false)
        {
            Validate.IsTrue(o != null ^ isStatic);
            Type t = isStatic ? typeof(T) : o.GetType();
            BindingFlags bindingFlags = GetBindingFlagsFromMethodQualifiers(isPublic, isStatic);
            FieldInfo fieldInfo = t.GetField(fieldName, bindingFlags);
            Validate.NotNull(fieldInfo, $"Class \"{t.Name}\" does not have a field called \"{fieldName}\".");
            return fieldInfo.GetValue(o);
        }

        public static void ReflectionSet<T>(this T o, string fieldName, object value, bool isPublic = false, bool isStatic = false)
        {
            Validate.IsTrue(o != null ^ isStatic);
            Type t = isStatic ? typeof(T) : o.GetType();
            BindingFlags bindingFlags = GetBindingFlagsFromMethodQualifiers(isPublic, isStatic);
            FieldInfo fieldInfo = t.GetField(fieldName, bindingFlags);
            Validate.NotNull(fieldInfo, $"Class \"{t.Name}\" does not have a field called \"{fieldName}\".");
            fieldInfo.SetValue(o, value);
        }

        public static object ReflectionGet<T, T2>(this T2 o, string fieldName, bool isPublic = false, bool isStatic = false)
            where T2 : T
        {
            Validate.IsTrue(o != null ^ isStatic);
            Type t = typeof(T);
            BindingFlags bindingFlags = GetBindingFlagsFromMethodQualifiers(isPublic, isStatic);
            FieldInfo fieldInfo = t.GetField(fieldName, bindingFlags);
            Validate.NotNull(fieldInfo, $"Class \"{t.Name}\" does not have a field called \"{fieldName}\".");
            return fieldInfo.GetValue(o);
        }

        public static void ReflectionSet<T, T2>(this T2 o, string fieldName, object value, bool isPublic = false, bool isStatic = false)
            where T2 : T
        {
            Validate.IsTrue(o != null ^ isStatic);
            Type t = typeof(T);
            BindingFlags bindingFlags = GetBindingFlagsFromMethodQualifiers(isPublic, isStatic);
            FieldInfo fieldInfo = t.GetField(fieldName, bindingFlags);
            Validate.NotNull(fieldInfo, $"Class \"{t.Name}\" does not have a field called \"{fieldName}\".");
            fieldInfo.SetValue(o, value);
        }

        private static BindingFlags GetBindingFlagsFromMethodQualifiers(bool isPublic, bool isStatic)
        {
            BindingFlags bindingFlags = isPublic ? BindingFlags.Public : BindingFlags.NonPublic;
            bindingFlags |= isStatic ? BindingFlags.Static : BindingFlags.Instance;

            return bindingFlags;
        }
    }
}

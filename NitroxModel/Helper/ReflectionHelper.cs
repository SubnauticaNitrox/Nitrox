using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NitroxModel.Helper
{
    public static class ReflectionHelper
    {
        // Public calls are useful for reflected, inaccessible objects.
        public static object ReflectionCall(this object o, string methodName, bool isPublic = false, bool isStatic = false, params object[] args)
        {
            BindingFlags bindingFlags = GetBindingFlagsFromMethodQualifiers(isPublic, isStatic);
            MethodInfo methodInfo = o.GetType().GetMethod(methodName, bindingFlags);
            Validate.NotNull(methodInfo, $"Class \"{o.GetType().Name}\" does not have a method called \"{methodName}\".");
            return methodInfo.Invoke(o, args);
        }

        public static object ReflectionCall(Type type, string methodName, Type[] types, bool isPublic = false, bool isStatic = false, params object[] args)
        {
            BindingFlags bindingFlags = GetBindingFlagsFromMethodQualifiers(isPublic, isStatic);
            MethodInfo methodInfo = type.GetMethod(methodName, bindingFlags, null, types, null);
            Validate.NotNull(methodInfo, $"Class \"{type.Name}\" does not have a method called \"{methodName}\".");
            return methodInfo.Invoke(null, args);
        }

        public static object ReflectionGet(this object o, string fieldName, bool isPublic = false, bool isStatic = false)
        {
            BindingFlags bindingFlags = GetBindingFlagsFromMethodQualifiers(isPublic, isStatic);
            FieldInfo fieldInfo = o.GetType().GetField(fieldName, bindingFlags);
            Validate.NotNull(fieldInfo, $"Class \"{o.GetType().Name}\" does not have a field called \"{fieldName}\".");
            return fieldInfo.GetValue(o);
        }

        public static void ReflectionSet(this object o, string fieldName, object value, bool isPublic = false, bool isStatic = false)
        {
            BindingFlags bindingFlags = GetBindingFlagsFromMethodQualifiers(isPublic, isStatic);
            FieldInfo fieldInfo = o.GetType().GetField(fieldName, bindingFlags);
            Validate.NotNull(fieldInfo, $"Class \"{o.GetType().Name}\" does not have a field called \"{fieldName}\".");
            fieldInfo.SetValue(o, value);
        }

        private static BindingFlags GetBindingFlagsFromMethodQualifiers(bool isPublic, bool isStatic)
        {
            BindingFlags bindingFlags = System.Reflection.BindingFlags.NonPublic;
            if (isPublic)
            {
                bindingFlags = System.Reflection.BindingFlags.Public;
            }

            if (isStatic)
            {
                bindingFlags = bindingFlags | System.Reflection.BindingFlags.Static;
            }
            else
            {
                bindingFlags = bindingFlags | System.Reflection.BindingFlags.Instance;
            }

            return bindingFlags;
        }
    }
}

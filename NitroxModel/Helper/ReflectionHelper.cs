using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NitroxModel.Helper
{
    public static class ReflectionHelper
    {
        public static object ReflectionCall(this object o, string methodName, bool publicMethod = false, params object[] args)
        {
            MethodInfo methodInfo = o.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Validate.NotNull(methodInfo, $"Class \"{o.GetType().Name}\" does not have a method called \"{methodName}\".");
            return methodInfo.Invoke(o, args);
        }

        public static object ReflectionGet(this object o, string fieldName)
        {
            FieldInfo fieldInfo = o.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Validate.NotNull(fieldInfo, $"Class \"{o.GetType().Name}\" does not have a field called \"{fieldName}\".");
            return fieldInfo.GetValue(o);
        }

        public static void ReflectionSet(this object o, string fieldName, object value)
        {
            FieldInfo fieldInfo = o.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Validate.NotNull(fieldInfo, $"Class \"{o.GetType().Name}\" does not have a field called \"{fieldName}\".");
            fieldInfo.SetValue(o, value);
        }
    }
}

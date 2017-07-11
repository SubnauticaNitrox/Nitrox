using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxPatcher
{
    public static class ReflectionHelper
    {
        public static object ReflectionCall(this object o, string methodName, params object[] args)
        {
            var mi = o.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (mi != null)
            {
                return mi.Invoke(o, args);
            }
            return null;
        }

        public static bool ReflectionSet(this object o, string fieldName, object value)
        {
            var fi = o.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fi != null)
            {
                fi.SetValue(o, value);
                return true;
            }
            return false;
        }
    }
}

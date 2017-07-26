using System;
using System.Linq;
using System.Reflection;

namespace NitroxReloader
{
    public static class ReloaderHelper
    {
        public static T GetAttribute<T>(this MethodBase method)
            where T : Attribute
        {
            return method.GetCustomAttributes(false).Where(a => a is T).Cast<T>().FirstOrDefault();
        }
    }
}

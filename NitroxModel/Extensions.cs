using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxModel
{
    public static class Extensions
    {
        public static TAttribute GetAttribute<TAttribute>(this Enum value)
            where TAttribute : Attribute
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);

            return type.GetField(name)
                .GetCustomAttributes(false)
                .OfType<TAttribute>()
                .SingleOrDefault();
        }

        public static string PrefixWith<T>(this IEnumerable<T> items, string prefix)
        {
            StringBuilder sb = new StringBuilder();
            foreach (T i in items)
            {
                sb.Append(prefix);
                sb.Append(i);
            }
            return sb.ToString();
        }
    }
}

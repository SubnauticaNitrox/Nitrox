using System;
using System.Collections.Generic;
using System.Linq;

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

        public static IEnumerable<T> TakeUntilLast<T>(this IEnumerable<T> source)
        {
            using IEnumerator<T> enumerator = source.GetEnumerator();
            bool first = true;

            T prev = default;
            while (enumerator.MoveNext())
            {
                if (!first)
                {
                    yield return prev;
                }
                first = false;
                prev = enumerator.Current;
            }
        }
    }
}

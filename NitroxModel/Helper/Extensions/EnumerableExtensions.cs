using System.Collections.Generic;
using System.Text;

namespace NitroxModel.Helper.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        ///     Joins all items as strings while prefixing them.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="prefix"></param>
        /// <returns>Joined string representations of the items with prefix per item.</returns>
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

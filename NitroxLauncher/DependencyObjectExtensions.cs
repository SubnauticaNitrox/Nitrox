using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace NitroxLauncher
{
    public static class DependencyObjectExtensions
    {
        /// <summary>
        ///     Gets the children of the given dependency object if the are an instance of the given type.
        /// </summary>
        /// <param name="depObj"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetChildrenOfType<T>(this DependencyObject depObj)
            where T : DependencyObject
        {
            if (depObj == null)
            {
                yield break;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                if (!(child is T))
                {
                    continue;
                }

                yield return (T)child;
                foreach (T grandChild in GetChildrenOfType<T>(child))
                {
                    yield return grandChild;
                }
            }
        }
    }
}

using System.Text;
using UnityEngine;

namespace NitroxModel.Helper.Extensions
{
    public static class GameObjectExtensions
    {
        public static string DumpAsString(this GameObject gameObject, string indent = "", bool dumpTransform = true)
        {
            StringBuilder builder = new StringBuilder();
            gameObject.DumpGameObjectInternal(builder, indent, dumpTransform);
            return builder.ToString();
        }

        private static void DumpGameObjectInternal(this GameObject gameObject, StringBuilder builder, string indent = "", bool dumpTransform = true)
        {
            builder.AppendFormat("{0}+{1}", indent, gameObject.name);

            if (dumpTransform)
            {
                foreach (Component component in gameObject.GetComponents<Component>())
                {
                    DumpComponentInternal(component, builder, indent + "  ");
                }
            }

            foreach (Transform child in gameObject.transform)
            {
                DumpGameObjectInternal(child.gameObject, builder, indent + "  ", dumpTransform);
            }
        }

        private static void DumpComponentInternal(this Component component, StringBuilder builder, string indent = "")
        {
            builder.AppendFormat("{0}{1}", indent, component == null ? "(null)" : component.GetType().Name + ": " + component);
        }
    }
}

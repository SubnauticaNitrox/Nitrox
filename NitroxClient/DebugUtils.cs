using System;
using System.Text;
using UnityEngine;

namespace NitroxClient
{
    public class DebugUtils
    {
        public static void DumpGameObject(GameObject gameObject, string indent)
        {
            Console.WriteLine("{0}+{1}", indent, gameObject.name);

            foreach (Component component in gameObject.GetComponents<Component>())
            {
                DumpComponent(component, indent + "  ");
            }

            foreach (Transform child in gameObject.transform)
            {
                DumpGameObject(child.gameObject, indent + "  ");
            }
        }

        public static void DumpComponent(Component component, string indent)
        {
            Console.WriteLine("{0}{1}", indent, (component == null ? "(null)" : component.GetType().Name));
        }

        public static String ByteArrayToHexString(byte[] bytes)
        {
            StringBuilder hex = new StringBuilder(bytes.Length * 2);

            foreach (byte b in bytes)
            {
                hex.Append("0x");
                hex.Append(b.ToString("X2"));
                hex.Append(" ");
            }

            return hex.ToString();
        }
    }
}

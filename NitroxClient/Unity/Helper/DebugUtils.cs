using NitroxModel.Logger;
using System;
using System.Text;
using UnityEngine;

namespace NitroxClient.Unity.Helper
{
    public class DebugUtils
    {
        private static string prefix = "";

        public static void DumpHierarchy(GameObject obj)
        {
            Log.Debug("Analyze gameobject: ");
            Log.Debug("------------------------------------");
            StartHierachy(obj);
            Log.Debug("------------------------------------");
        }

        private static void StartHierachy(GameObject obj)
        {
            prefix = prefix + "    ";
            Log.Debug(GetInfoOfGamobject(obj));
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                StartHierachy(obj.transform.GetChild(i).gameObject);
                prefix = prefix.Substring(4);
            }
        }

        private static string GetInfoOfGamobject(GameObject obj)
        {
            return prefix + obj.name + "   --<>--   " + GetComponentsOfGamobject(obj);
        }

        private static string GetComponentsOfGamobject(GameObject obj)
        {
            string list = "";
            foreach (Component item in obj.GetComponents<Component>())
            {
                list += item.GetType() + " | ";
            }
            if (list.Length >= 3)
            {
                list = list.Substring(0, list.Length-3);
            }
            return list;
        }

        public static void DumpGameObject(GameObject gameObject, string indent = "")
        {
            Log.Info("{0}+{1}", indent, gameObject.name);

            foreach (Component component in gameObject.GetComponents<Component>())
            {
                DumpComponent(component, indent + "  ");
            }

            foreach (Transform child in gameObject.transform)
            {
                DumpGameObject(child.gameObject, indent + "  ");
            }
        }

        public static void DumpComponent(Component component, string indent = "")
        {
            Log.Info("{0}{1}", indent, (component == null ? "(null)" : component.GetType().Name));
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

using NitroxModel.Logger;
using UnityEngine;

namespace NitroxClient.Unity.Helper
{
    public static class DebugUtils
    {
        public static void DumpGameObject(this GameObject gameObject, string indent = "", bool dumpTransform = true)
        {
            Log.Info("{0}+{1}", indent, gameObject.name);

            if (dumpTransform)
            {
                foreach (Component component in gameObject.GetComponents<Component>())
                {
                    DumpComponent(component, indent + "  ");
                }
            }

            foreach (Transform child in gameObject.transform)
            {
                DumpGameObject(child.gameObject, indent + "  ", dumpTransform);
            }
        }

        public static void DumpComponent(this Component component, string indent = "")
        {
            Log.Info("{0}{1}", indent, (component == null ? "(null)" : component.GetType().Name + ": " + component.ToString()));
        }
    }
}

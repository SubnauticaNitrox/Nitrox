using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NitroxClient
{
    public class DebugUtils
    {
        private static void DumpGameObject(GameObject gameObject, string indent)
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

        private static void DumpComponent(Component component, string indent)
        {
            Console.WriteLine("{0}{1}", indent, (component == null ? "(null)" : component.GetType().Name));
        }
    }
}

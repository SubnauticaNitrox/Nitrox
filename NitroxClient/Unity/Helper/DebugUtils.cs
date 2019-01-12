using NitroxClient.GameLogic.Helper;
using NitroxModel.Logger;
using UnityEngine;
using System;

namespace NitroxClient.Unity.Helper
{
    public static class DebugUtils
    {
        [Obsolete("Please use PrintHierarchy")]
        public static void DumpGameObject(this GameObject gameObject, string indent = "", bool dumpTransform = true)
        {
            PrintHierarchy(gameObject, false, 0, true);
        }

        [Obsolete("Please use PrintHierarchy")]
        public static void DumpComponent(this Component component, string indent = "")
        {
            Log.Info("{0}{1}", indent, (component == null ? "(null)" : component.GetType().Name + ": " + component.ToString()));
        }

        public static void PrintHierarchy(GameObject gameObject, bool startAtRoot = false, int parentsUpwards = 1, bool listComponents = false, bool travelDown = true)
        {
            GameObject startHierarchy = gameObject;
            if (startAtRoot)
            {
                GameObject rootObject = gameObject.transform.root.gameObject;
                if (rootObject != null)
                {
                    startHierarchy = rootObject;
                }
            }
            else
            {
                GameObject parentObject = gameObject;
                int i = 0;
                while (i < parentsUpwards)
                {
                    i++;
                    if (parentObject.transform.parent != null)
                    {
                        parentObject = parentObject.transform.parent.gameObject;
                    }
                    else
                    {
                        i = parentsUpwards;
                    }
                }
            }

            TravelDown(startHierarchy, listComponents, "", travelDown);
        }

        private static void TravelDown(GameObject gameObject, bool listComponents = false, string linePrefix = "", bool travelDown = true)
        {
            Log.Debug("{0}+GameObject GUID={1} NAME={2}", linePrefix, GuidHelper.GetGuid(gameObject), gameObject.name);
            if (listComponents)
            {
                ListComponents(gameObject, linePrefix);
            }

            if (!travelDown)
            {
                return;
            }
            foreach (Transform child in gameObject.transform)
            {
                TravelDown(child.gameObject, listComponents, linePrefix + "|  ");
            }
        }

        private static void ListComponents(GameObject gameObject, string linePrefix = "")
        {
            Component[] allComponents = gameObject.GetComponents<Component>();
            foreach (Component c in allComponents)
            {
                Log.Debug("{0}   =Component NAME={1}", linePrefix, c.name);
            }
        }
    }
}

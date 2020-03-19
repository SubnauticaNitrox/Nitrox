using NitroxModel.Logger;
using UnityEngine;
using NitroxClient.MonoBehaviours;

namespace NitroxClient.Unity.Helper
{
    public static class DebugUtils
    {
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
            Log.Debug("{0}+GameObject GUID={1} NAME={2} POSITION={3}", linePrefix, NitroxEntity.GetId(gameObject), gameObject.name, gameObject.transform.position);
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
                Log.Debug("{0}   =Component NAME={1}", linePrefix, c.GetType().Name);
            }
        }
    }
}

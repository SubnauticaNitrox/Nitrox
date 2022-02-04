using NitroxClient.MonoBehaviours;
using UnityEngine;

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
            NitroxEntity entity = gameObject.GetComponent<NitroxEntity>();
            string guid = (entity) ? entity.Id.ToString() : "None";

            Log.Debug($"{linePrefix}+GameObject GUID={guid} NAME={gameObject.name} POSITION={gameObject.transform.position}");
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
                Log.Debug($"{linePrefix}=Component NAME={c.GetType().Name}");
            }
        }
    }
}

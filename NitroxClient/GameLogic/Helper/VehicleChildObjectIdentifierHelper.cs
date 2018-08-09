using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxClient.GameLogic.Helper
{
    public class VehicleChildObjectIdentifierHelper
    {
        private static readonly List<Type> interactiveChildTypes = new List<Type>() // we must sync guids of these types when creating vehicles (mainly cyclops)
        {
            { typeof(Openable) },
            { typeof(CyclopsLocker) },
            { typeof(Fabricator) },
            { typeof(FireExtinguisherHolder) },
            { typeof(StorageContainer) },
            { typeof(SeamothStorageContainer) },
            { typeof(VehicleDockingBay) },
            { typeof(DockedVehicleHandTarget) },
            { typeof(DockingBayDoor) }
        };

        public static  List<InteractiveChildObjectIdentifier> ExtractGuidsOfInteractiveChildren(GameObject constructedObject)
        {
            List<InteractiveChildObjectIdentifier> ids = new List<InteractiveChildObjectIdentifier>();

            string constructedObjectsName = constructedObject.GetFullName() + "/";

            foreach (Type type in interactiveChildTypes)
            {
                Component[] components = constructedObject.GetComponentsInChildren(type, true);

                foreach (Component component in components)
                {
                    string guid = GuidHelper.GetGuid(component.gameObject);
                    string componentName = component.gameObject.GetFullName();
                    string relativePathName = componentName.Replace(constructedObjectsName, "");

                    ids.Add(new InteractiveChildObjectIdentifier(guid, relativePathName));
                }
            }

            return ids;
        }

        public static void SetInteractiveChildrenGuids(GameObject constructedObject, List<InteractiveChildObjectIdentifier> interactiveChildIdentifiers)
        {
            foreach (InteractiveChildObjectIdentifier childIdentifier in interactiveChildIdentifiers)
            {
                Transform transform = constructedObject.transform.Find(childIdentifier.GameObjectNamePath);

                if (transform != null)
                {
                    GameObject gameObject = transform.gameObject;
                    GuidHelper.SetNewGuid(gameObject, childIdentifier.Guid);
                }
                else
                {
                    Log.Error("Error GUID tagging interactive child due to not finding it: " + childIdentifier.GameObjectNamePath);
                }
            }
        }
    }
}

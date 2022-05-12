using System;
using System.Collections.Generic;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using UnityEngine;

namespace NitroxClient.GameLogic.Helper
{
    public class VehicleChildObjectIdentifierHelper
    {
        private static readonly List<Type> interactiveChildTypes = new List<Type> // we must sync ids of these types when creating vehicles (mainly cyclops)
        {
            typeof(Openable),
            typeof(CyclopsLocker),
            typeof(Fabricator),
            typeof(FireExtinguisherHolder),
            typeof(StorageContainer),
            typeof(SeamothStorageContainer),
            typeof(VehicleDockingBay),
            typeof(DockedVehicleHandTarget),
            typeof(UpgradeConsole),
            typeof(DockingBayDoor),
            typeof(CyclopsDecoyLoadingTube),
            typeof(BatterySource),
            typeof(EnergyMixin)
        };

        public static List<InteractiveChildObjectIdentifier> ExtractInteractiveChildren(GameObject constructedObject)
        {
            List<InteractiveChildObjectIdentifier> interactiveChildren = new List<InteractiveChildObjectIdentifier>();

            string constructedObjectsName = constructedObject.GetFullHierarchyPath();

            foreach (Type type in interactiveChildTypes)
            {
                Component[] components = constructedObject.GetComponentsInChildren(type, true);

                foreach (Component component in components)
                {
                    NitroxId id = NitroxEntity.GetId(component.gameObject);
                    string componentName = component.gameObject.GetFullHierarchyPath();
                    string relativePathName = componentName.Replace(constructedObjectsName, "");

                    // It can happen, that the game object is the constructed object itself. This code prevents to add itself to the child objects
                    if (relativePathName.Length != 0)
                    {
                        relativePathName = relativePathName.TrimStart('/');
                        interactiveChildren.Add(new InteractiveChildObjectIdentifier(id, relativePathName));
                    }
                }
            }

            return interactiveChildren;
        }

        public static void SetInteractiveChildrenIds(GameObject constructedObject, IEnumerable<InteractiveChildObjectIdentifier> interactiveChildIdentifiers)
        {
            foreach (InteractiveChildObjectIdentifier childIdentifier in interactiveChildIdentifiers)
            {
                Transform transform = constructedObject.transform.Find(childIdentifier.GameObjectNamePath);

                if (transform != null)
                {
                    GameObject gameObject = transform.gameObject;
                    NitroxEntity.SetNewId(gameObject, childIdentifier.Id);
                }
                else
                {
                    Log.Error("Error GUID tagging interactive child due to not finding it: " + childIdentifier.GameObjectNamePath);
                }
            }
        }
    }
}

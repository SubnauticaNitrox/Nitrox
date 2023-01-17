using System;
using System.Collections.Generic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using UnityEngine;

namespace NitroxClient.GameLogic.Helper;

public class VehicleChildEntityHelper
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

    public static List<Entity> ExtractChildren(GameObject vehicle)
    {
        NitroxId parentId = NitroxEntity.GetId(vehicle);
        List<Entity> children = new List<Entity>();

        string constructedObjectsName = vehicle.GetFullHierarchyPath();

        foreach (Type type in interactiveChildTypes)
        {
            Component[] components = vehicle.GetComponentsInChildren(type, true);

            for(int i = 0; i < components.Length; i++)
            {
                Component component = components[i];

                NitroxId id = NitroxEntity.GetId(component.gameObject);
                string componentName = component.gameObject.GetFullHierarchyPath();
                string relativePathName = componentName.Replace(constructedObjectsName, "");

                // It can happen, that the game object is the constructed object itself. This code prevents to add itself to the child objects
                if (relativePathName.Length != 0)
                {
                    relativePathName = relativePathName.TrimStart('/');
                    children.Add(new PathBasedChildEntity(relativePathName, id, null, null, parentId, new List<Entity>()));
                }
            }
        }

        return children;
    }
}


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
    private static readonly HashSet<Type> interactiveChildTypes = new HashSet<Type> // we must sync ids of these types when creating vehicles (mainly cyclops)
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
        typeof(EnergyMixin),
        typeof(SubNameInput),
        typeof(WeldablePoint),
        typeof(CyclopsVehicleStorageTerminalManager),
        typeof(CyclopsLightingPanel)
    };

    public static void PopulateChildren(NitroxId vehicleId, string vehiclePath, List<Entity> toPopulate, GameObject current)
    {
        string currentPath = current.GetFullHierarchyPath();
        string relativePathName = currentPath.Replace(vehiclePath, "")
                                             .TrimStart('/');

        if (relativePathName.Length > 0) // no need to execute for the main vehicle.
        {
            foreach (MonoBehaviour mono in current.GetComponents<MonoBehaviour>())
            {
                if (interactiveChildTypes.Contains(mono.GetType()))
                {
                    // We don't to accidentally tag this game object unless we know it has an applicable mono
                    NitroxId id = NitroxEntity.RequireIdFrom(mono.gameObject);
                    toPopulate.Add(new PathBasedChildEntity(relativePathName, id, null, null, vehicleId, new()));
                }
            }
        }

        foreach (Transform child in current.transform)
        {
            PopulateChildren(vehicleId, vehiclePath, toPopulate, child.gameObject);
        }
    }
}

using System;
using System.Collections.Generic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using UnityEngine;

namespace NitroxClient.GameLogic.Helper;

public static class VehicleChildEntityHelper
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
        typeof(SubNameInput),
        typeof(WeldablePoint),
        typeof(CyclopsVehicleStorageTerminalManager),
        typeof(CyclopsLightingPanel)
    };

    public static void PopulateChildren(NitroxId vehicleId, string vehiclePath, List<Entity> toPopulate, GameObject current)
    {
        string currentPath = current.GetFullHierarchyPath();
        string relativePathName = currentPath.Replace(vehiclePath, string.Empty).TrimStart('/');

        if (relativePathName.Length > 0)
        {
            // generate PathBasedChildEntities for gameObjects under the main vehicle.
            foreach (MonoBehaviour mono in current.GetComponents<MonoBehaviour>())
            {
                // We don't to accidentally tag this game object unless we know it has an applicable mono
                if (interactiveChildTypes.Contains(mono.GetType()))
                {
                    NitroxId id = NitroxEntity.GetIdOrGenerateNew(mono.gameObject);

                    PathBasedChildEntity pathBasedChildEntity = new(relativePathName, id, null, null, vehicleId, new());
                    toPopulate.Add(pathBasedChildEntity);

                    if (mono is BatterySource batterySource) // cyclops has a battery source as a deeply-nested child
                    {
                        BatteryChildEntityHelper.PopulateInstalledBattery(batterySource, pathBasedChildEntity.ChildEntities, id);
                    }
                }
            }
        }
        else
        {
            // both seamoth and exosuit have energymixin as a direct component. populate the battery if it exists
            BatteryChildEntityHelper.TryPopulateInstalledBattery(current, toPopulate, vehicleId);
        }

        foreach (Transform child in current.transform)
        {
            PopulateChildren(vehicleId, vehiclePath, toPopulate, child.gameObject);
        }
    }
}

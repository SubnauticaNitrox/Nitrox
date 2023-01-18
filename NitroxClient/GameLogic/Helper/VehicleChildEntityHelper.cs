using System;
using System.Collections.Generic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using UWE;

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
        typeof(SubNameInput),
        typeof(WeldablePoint),
        typeof(CyclopsVehicleStorageTerminalManager),
        typeof(CyclopsLightingPanel)
    };

    public static void PopulateChildren(NitroxId vehicleId, string vehiclePath, List<Entity> toPopulate, GameObject current, Entities entities)
    {
        string currentPath = current.GetFullHierarchyPath();
        string relativePathName = currentPath.Replace(vehiclePath, "")
                                             .TrimStart('/');

        if (relativePathName.Length > 0) 
        {
            // generate PathBasedChildEntities for gameObjects under the main vehicle.
            foreach (MonoBehaviour mono in current.GetComponents<MonoBehaviour>())
            {
                if (interactiveChildTypes.Contains(mono.GetType()))
                {
                    // We don't to accidentally tag this game object unless we know it has an applicable mono
                    NitroxId id = NitroxEntity.GetId(mono.gameObject);

                    PathBasedChildEntity pathBasedChildEntity = new(relativePathName, id, null, null, vehicleId, new());
                    toPopulate.Add(pathBasedChildEntity);

                    if (mono is BatterySource energyMixin) // cyclops has a battery source as a deeply-nested child
                    {
                        PopulateInstalledBattery(energyMixin, pathBasedChildEntity.ChildEntities, id, entities);
                    }
                }
            }
        }
        else if (current.TryGetComponent(out EnergyMixin vehicleEnergyMixin))
        {
            // both seamoth and exosuit have energymixin as a direct component. populate the battery if it exists
            PopulateInstalledBattery(vehicleEnergyMixin, toPopulate, vehicleId, entities);
        }

        foreach (Transform child in current.transform)
        {
            PopulateChildren(vehicleId, vehiclePath, toPopulate, child.gameObject, entities);
        }
    }

    // Vehicles are created without a battery loaded into them.  Subnautica usually spawns these in async; however, this is
    // disabled in nitrox so we can properly tag the id.  Here we create the installed battery (with a new NitroxId) and have
    // the entity spawner take care of loading it in.
    public static void PopulateInstalledBattery(EnergyMixin energyMixin, List<Entity> toPopulate, NitroxId parentId, Entities entities)
    {
        InstalledBatteryEntity installedBattery = new(new NitroxId(), energyMixin.defaultBattery.ToDto(), null, parentId, new List<Entity>());
        toPopulate.Add(installedBattery);

        CoroutineHost.StartCoroutine(entities.SpawnAsync(installedBattery));
    }
}


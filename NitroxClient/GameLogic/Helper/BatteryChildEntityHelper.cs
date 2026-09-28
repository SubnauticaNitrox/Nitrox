using System;
using System.Collections.Generic;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;
using UWE;

namespace NitroxClient.GameLogic.Helper;

/// <summary>
/// Vehicles and items are created without a battery loaded into them. Subnautica usually spawns these in async; however, this
/// is disabled in nitrox so we can properly tag the id. Here we create the installed battery (with a new NitroxId) and have the
/// entity spawner take care of loading it in.
/// </summary>
public static class BatteryChildEntityHelper
{
    private static readonly Lazy<Entities> entities = new (() => NitroxServiceLocator.LocateService<Entities>());
    private static readonly Lazy<EntityMetadataManager> defaultMetadataManager = new (() => NitroxServiceLocator.LocateService<EntityMetadataManager>());

    /// <summary>
    /// Tracks installed batteries as soon as they're known (spawn dispatched), keyed by the owning entity's id and the
    /// <see cref="EnergyMixin"/>'s component index. This bridges the (usually sub-frame, but non-zero) window between an
    /// <see cref="InstalledBatteryEntity"/> being known and its backing GameObject actually finishing its (async) spawn and
    /// being wired into <see cref="EnergyMixin.batterySlot"/>. Without this, a pickup happening in that window - most notably
    /// another player picking up a just-received dropped item - would wrongly conclude the tool has no battery.
    /// </summary>
    private static readonly Dictionary<(NitroxId ParentId, int ComponentIndex), InstalledBatteryEntity> pendingInstalledBatteries = [];

    /// <summary>
    /// Overload for callers (e.g. vehicle construction) that don't have an <see cref="EntityMetadataManager"/> instance at hand.
    /// </summary>
    public static void TryPopulateInstalledBattery(GameObject gameObject, List<Entity> toPopulate, NitroxId parentId, bool allowDefaultBattery = true)
    {
        TryPopulateInstalledBattery(gameObject, toPopulate, parentId, defaultMetadataManager.Value, allowDefaultBattery);
    }

    public static void TryPopulateInstalledBattery(GameObject gameObject, List<Entity> toPopulate, NitroxId parentId, EntityMetadataManager entityMetadataManager, bool allowDefaultBattery = true)
    {
        if (gameObject.TryGetComponent(out EnergyMixin energyMixin))
        {
            PopulateInstalledBatteryInternal(energyMixin, toPopulate, parentId, entityMetadataManager, allowDefaultBattery);
        }
    }

    /// <remarks>Used to unconditionally populate a fresh default battery for a given EnergyMixin, e.g. a cyclops BatterySource during vehicle construction.</remarks>
    public static void PopulateInstalledBattery(EnergyMixin energyMixin, List<Entity> toPopulate, NitroxId parentId)
    {
        PopulateInstalledBatteryInternal(energyMixin, toPopulate, parentId, defaultMetadataManager.Value, allowDefaultBattery: true);
    }

    private static void PopulateInstalledBatteryInternal(EnergyMixin energyMixin, List<Entity> toPopulate, NitroxId parentId, EntityMetadataManager entityMetadataManager, bool allowDefaultBattery)
    {
        int componentIndex = GetComponentIndex(energyMixin, parentId);
        InventoryItem storedItem = energyMixin.batterySlot?.storedItem;

        if (storedItem?.item)
        {
            // Reflect the actual currently-installed battery (its real id, tech type and metadata/charge) instead of
            // assuming a default one. This is what makes dropping/picking up a tool with a real battery installed
            // (including by a different player than the one who installed it) preserve that exact battery.
            NitroxId batteryId = NitroxEntity.GetIdOrGenerateNew(storedItem.item.gameObject);
            Optional<EntityMetadata> metadata = entityMetadataManager.Extract(storedItem.item.gameObject);
            toPopulate.Add(new InstalledBatteryEntity(componentIndex, batteryId, storedItem.item.GetTechType().ToDto(), metadata.OrNull(), parentId, []));
            ForgetPendingInstalledBattery(parentId, componentIndex);
        }
        else if (TryGetPendingInstalledBattery(parentId, componentIndex, out InstalledBatteryEntity pendingBattery))
        {
            toPopulate.Add(pendingBattery);
        }
        else if (allowDefaultBattery)
        {
            PopulateDefaultInstalledBattery(energyMixin, componentIndex, toPopulate, parentId);
        }
        // Otherwise: no battery installed, and none allowed to be assumed (e.g. an already-known, previously-emptied tool).
    }

    private static void PopulateDefaultInstalledBattery(EnergyMixin energyMixin, int componentIndex, List<Entity> toPopulate, NitroxId parentId)
    {
        InstalledBatteryEntity installedBattery = new(componentIndex, new NitroxId(), energyMixin.defaultBattery.ToDto(), null, parentId, []);
        toPopulate.Add(installedBattery);

        CoroutineHost.StartCoroutine(entities.Value.SpawnEntityAsync(installedBattery));
    }

    /// <summary>
    /// Records that the given <see cref="InstalledBatteryEntity"/> is about to be spawned in, before waiting on any
    /// asynchronous spawn of its backing GameObject.
    /// </summary>
    public static void RegisterPendingInstalledBattery(InstalledBatteryEntity installedBattery)
    {
        pendingInstalledBatteries[(installedBattery.ParentId, installedBattery.ComponentIndex)] = installedBattery;
    }

    /// <summary>
    /// Forgets a previously pending installed battery, whether because its spawn finished (its state is now correctly
    /// reflected by <see cref="EnergyMixin.batterySlot"/>) or because it was actually removed from its slot.
    /// </summary>
    public static void ForgetPendingInstalledBattery(NitroxId parentId, int componentIndex)
    {
        pendingInstalledBatteries.Remove((parentId, componentIndex));
    }

    public static void ForgetPendingInstalledBattery(EnergyMixin energyMixin, NitroxId parentId)
    {
        ForgetPendingInstalledBattery(parentId, GetComponentIndex(energyMixin, parentId));
    }

    private static bool TryGetPendingInstalledBattery(NitroxId parentId, int componentIndex, out InstalledBatteryEntity installedBattery)
    {
        return pendingInstalledBatteries.TryGetValue((parentId, componentIndex), out installedBattery);
    }

    private static int GetComponentIndex(EnergyMixin energyMixin, NitroxId parentId)
    {
        EnergyMixin[] components = NitroxEntity.RequireObjectFrom(parentId).GetAllComponentsInChildren<EnergyMixin>();
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] == energyMixin)
            {
                return i;
            }
        }

        return 0;
    }
}

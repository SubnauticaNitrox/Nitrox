using System.Collections;
using System.Linq;
using Nitrox.Model.DataStructures;
using NitroxClient.Communication;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.GameLogic.Spawning.WorldEntities;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning;

public class InstalledBatteryEntitySpawner : SyncEntitySpawner<InstalledBatteryEntity>
{
    protected override IEnumerator SpawnAsync(InstalledBatteryEntity entity, TaskResult<Optional<GameObject>> result)
    {
        if (!CanSpawn(entity, out EnergyMixin energyMixin, out string errorLog))
        {
            Log.Error(errorLog);
            result.Set(Optional.Empty);
            yield break;
        }

        TaskResult<GameObject> prefabResult = new();
        yield return DefaultWorldEntitySpawner.RequestPrefab(entity.TechType.ToUnity(), prefabResult);
        GameObject gameObject = GameObjectExtensions.InstantiateWithId(prefabResult.Get(), entity.Id);

        SetupObject(gameObject, energyMixin);

        result.Set(gameObject);
    }

    protected override bool SpawnSync(InstalledBatteryEntity entity, TaskResult<Optional<GameObject>> result)
    {
        if (!DefaultWorldEntitySpawner.TryGetCachedPrefab(out GameObject prefab, entity.TechType.ToUnity()))
        {
            return false;
        }
        if (!CanSpawn(entity, out EnergyMixin energyMixin, out string errorLog))
        {
            Log.Error(errorLog);
            return true;
        }

        GameObject gameObject = GameObjectExtensions.SpawnFromPrefab(prefab, entity.Id);

        SetupObject(gameObject, energyMixin);

        result.Set(gameObject);
        return true;
    }

    protected override bool SpawnsOwnChildren(InstalledBatteryEntity entity) => false;

    private bool CanSpawn(InstalledBatteryEntity entity, out EnergyMixin energyMixin, out string errorLog)
    {
        if (!NitroxEntity.TryGetObjectFrom(entity.ParentId, out GameObject parentObject))
        {
            energyMixin = null;
            errorLog = $"Unable to find parent to install battery {entity}";
            return false;
        }

        energyMixin = parentObject.GetAllComponentsInChildren<EnergyMixin>()
                                  .ElementAt(entity.ComponentIndex);

        if (!energyMixin)
        {
            errorLog = $"Unable to find EnergyMixin on parent to install battery {entity}";
            return false;
        }
        errorLog = null;
        return true;
    }

    private void SetupObject(GameObject gameObject, EnergyMixin energyMixin)
    {
        energyMixin.Initialize();
        energyMixin.RestoreBattery();

        using (PacketSuppressor<EntityReparented>.Suppress())
        using (PacketSuppressor<EntitySpawnedByClient>.Suppress())
        {
            energyMixin.batterySlot.AddItem(new InventoryItem(gameObject.GetComponent<Pickupable>()));
        }
    }
}

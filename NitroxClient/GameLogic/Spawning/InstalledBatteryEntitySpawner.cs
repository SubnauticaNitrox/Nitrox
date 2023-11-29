using System.Collections;
using NitroxClient.Communication;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.GameLogic.Spawning.WorldEntities;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
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
        GameObject gameObject = UnityEngine.Object.Instantiate(prefabResult.Get());

        SetupObject(entity, gameObject, energyMixin);

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

        GameObject gameObject = Utils.SpawnFromPrefab(prefab, null);

        SetupObject(entity, gameObject, energyMixin);

        result.Set(gameObject);
        return true;
    }

    protected override bool SpawnsOwnChildren(InstalledBatteryEntity entity) => false;

    private bool CanSpawn(Entity entity, out EnergyMixin energyMixin, out string errorLog)
    {
        if (!NitroxEntity.TryGetObjectFrom(entity.ParentId, out GameObject parentObject))
        {
            energyMixin = null;
            errorLog = $"Unable to find parent to install battery {entity}";
            return false;
        }

        energyMixin = parentObject.GetComponent<EnergyMixin>();

        if (!energyMixin)
        {
            errorLog = $"Unable to find EnergyMixin on parent to install battery {entity}";
            return false;
        }
        errorLog = null;
        return true;
    }

    private void SetupObject(Entity entity, GameObject gameObject, EnergyMixin energyMixin)
    {
        energyMixin.Initialize();
        energyMixin.RestoreBattery();

        NitroxEntity.SetNewId(gameObject, entity.Id);

        using (PacketSuppressor<EntityReparented>.Suppress())
        using (PacketSuppressor<EntitySpawnedByClient>.Suppress())
        {
            energyMixin.batterySlot.AddItem(new InventoryItem(gameObject.GetComponent<Pickupable>()));
        }
    }
}

using System.Collections;
using NitroxClient.Communication;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning;

public class InstalledBatteryEntitySpawner : EntitySpawner<InstalledBatteryEntity>
{
    public override IEnumerator SpawnAsync(InstalledBatteryEntity entity, TaskResult<Optional<GameObject>> result)
    {
        Optional<GameObject> parent = NitroxEntity.GetObjectFrom(entity.ParentId);

        if (!parent.HasValue)
        {
            Log.Info($"Unable to find parent to install battery {entity}");
            result.Set(Optional.Empty);
            yield break;
        }

        EnergyMixin energyMixin = parent.Value.GetComponent<EnergyMixin>();

        if (!energyMixin)
        {
            Log.Info($"Unable to find EnergyMixin on parent to install battery {entity}");
            result.Set(Optional.Empty);
            yield break;
        }

        energyMixin.Initialize();
        energyMixin.RestoreBattery();

        CoroutineTask<GameObject> techPrefabCoroutine = CraftData.GetPrefabForTechTypeAsync(entity.TechType.ToUnity(), false);
        yield return techPrefabCoroutine;
        GameObject prefab = techPrefabCoroutine.GetResult();
        GameObject gameObject = UnityEngine.Object.Instantiate(prefab);

        NitroxEntity.SetNewId(gameObject, entity.Id);

        using (PacketSuppressor<EntityReparented>.Suppress())
        using (PacketSuppressor<EntitySpawnedByClient>.Suppress())
        {
            energyMixin.batterySlot.AddItem(new InventoryItem(gameObject.GetComponent<Pickupable>()));
        }

        result.Set(gameObject);
    }
 
    public override bool SpawnsOwnChildren(InstalledBatteryEntity entity)
    {
        return false;
    }
}

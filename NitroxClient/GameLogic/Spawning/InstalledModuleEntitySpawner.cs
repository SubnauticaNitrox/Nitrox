using System.Collections;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.Spawning.WorldEntities;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning;

public class InstalledModuleEntitySpawner : EntitySpawner<InstalledModuleEntity>
{
    public override IEnumerator SpawnAsync(InstalledModuleEntity entity, TaskResult<Optional<GameObject>> result)
    {
        Optional<GameObject> owner = NitroxEntity.GetObjectFrom(entity.ParentId);

        if (!owner.HasValue)
        {
            Log.Error($"Unable to find inventory container with id {entity.Id} for {entity}");
            result.Set(Optional.Empty);
            yield break;
        }

        TaskResult<GameObject> gameObjectResult = new();
        yield return DefaultWorldEntitySpawner.CreateGameObject(entity.TechType.ToUnity(), entity.ClassId, gameObjectResult);
        GameObject gameObject = gameObjectResult.Get();

        NitroxEntity.SetNewId(gameObject, entity.Id);

        // The game considers modules as vehicle equipment.  Get the container and install it into the required slot.
        Optional<Equipment> opEquipment = EquipmentHelper.FindEquipmentComponent(owner.Value);

        if (opEquipment.HasValue)
        {
            Pickupable pickupable = gameObject.RequireComponent<Pickupable>();
            pickupable.Initialize();

            Equipment equipment = opEquipment.Value;
            InventoryItem inventoryItem = new(pickupable);
            inventoryItem.container = equipment;
            inventoryItem.item.Reparent(equipment.tr);

            equipment.equipment[entity.Slot] = inventoryItem;

            equipment.UpdateCount(pickupable.GetTechType(), true);
            Equipment.SendEquipmentEvent(pickupable, 0, owner.Value, entity.Slot);
            equipment.NotifyEquip(entity.Slot, inventoryItem);
            result.Set(Optional.Of(gameObject));
        }
        else
        {
            Log.Error($"Unable to find equipment container inside {owner}");
            result.Set(Optional.Empty);
        }
    }
 
    public override bool SpawnsOwnChildren(InstalledModuleEntity entity)
    {
        return true;
    }
}

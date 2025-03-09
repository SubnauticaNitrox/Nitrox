using System;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class EntityReparentedProcessor : ClientPacketProcessor<EntityReparented>
{
    private readonly Entities entities;

    public EntityReparentedProcessor(Entities entities)
    {
        this.entities = entities;
    }

    public override void Process(EntityReparented packet)
    {
        Optional<GameObject> entity = NitroxEntity.GetObjectFrom(packet.Id);

        if (!entity.HasValue)
        {
            // In some cases, the affected entity may be pending spawning or out of range.
            // we only require the parent (in this case, the visible entity is undergoing
            // some change that must be shown, and if not is an error).
            return;
        }

        GameObject newParent = NitroxEntity.RequireObjectFrom(packet.NewParentId);

        if (entity.Value.TryGetComponent(out Pickupable pickupable))
        {
            WaterParkItem waterParkItem = pickupable.GetComponent<WaterParkItem>();
            // If the entity is being parented to a WaterPark
            if (newParent.TryGetComponent(out WaterPark waterPark))
            {
                // If the entity is already in a WaterPark
                if (waterParkItem.currentWaterPark)
                {
                    waterParkItem.SetWaterPark(waterPark);
                    return;
                }
                pickupable.SetVisible(false);
                pickupable.Activate(false);
                waterPark.AddItem(pickupable);
                // The reparenting is automatic here so we don't need to continue
                return;
            }
            // If the entity was parented to a WaterPark but is picked up by someone
            else if (waterParkItem)
            {
                pickupable.Deactivate();
                waterParkItem.SetWaterPark(null);
            }
        }

        using (PacketSuppressor<EntityReparented>.Suppress())
        {
            Type entityType = entities.RequireEntityType(packet.Id);

            // Move this to a resolver if there ends up being a lot of custom reparenting logic
            if (entityType == typeof(InventoryItemEntity))
            {
                InventoryItemReparented(entity.Value, newParent);
            }
            else
            {
                PerformDefaultReparenting(entity.Value, newParent);
            }
        }
    }

    private void InventoryItemReparented(GameObject entity, GameObject newParent)
    {
        Optional<ItemsContainer> opContainer = InventoryContainerHelper.TryGetContainerByOwner(newParent);

        if (!opContainer.HasValue)
        {
            Log.Error($"Could not find container field on GameObject {newParent.GetFullHierarchyPath()}");
            return;
        }

        Pickupable pickupable = entity.RequireComponent<Pickupable>();

        ItemsContainer container = opContainer.Value;
        container.UnsafeAdd(new InventoryItem(pickupable));
    }

    private void PerformDefaultReparenting(GameObject entity, GameObject newParent)
    {
        entity.transform.SetParent(newParent.transform, false);
    }
}

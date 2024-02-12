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
using static NitroxModel.DisplayStatusCodes;
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
        // Get the object that we are reparenting from the packet
        Optional<GameObject> entity = NitroxEntity.GetObjectFrom(packet.Id);

        if (!entity.HasValue)
        {
            // In some cases, the affected entity may be pending spawning or out of range.
            // we only require the parent (in this case, the visible entity is undergoing
            // some change that must be shown, and if not is an error).
            DisplayStatusCode(StatusCode.invalidPacket);
            return;
        }
        // Get the soon-to-be new parent of the gameObject from the packet
        GameObject newParent = NitroxEntity.RequireObjectFrom(packet.NewParentId);
        // If the entity is able to be picked up(is it an inventory item?)
        if (entity.Value.TryGetComponent(out Pickupable pickupable))
        {
            // If the entity is being parented to a WaterPark
            if (newParent.TryGetComponent(out WaterPark waterPark))
            {
                pickupable.SetVisible(false);
                pickupable.Activate(false);
                waterPark.AddItem(pickupable);
                // The reparenting is automatic here so we don't need to continue
                return;
            }
            // If the entity was parented to a WaterPark but is picked up by someone
            else if (pickupable.TryGetComponent(out WaterParkItem waterParkItem))
            {
                pickupable.Deactivate();
                waterParkItem.SetWaterPark(null);
            }
        }
        
        using (PacketSuppressor<EntityReparented>.Suppress())
        {
            // Get the type of the gameObject that we are reparenting
            Type entityType = entities.RequireEntityType(packet.Id);

            // Move this to a resolver if there ends up being a lot of custom reparenting logic
            // Use the appropriate method based on whether the entity is an inventoryItem or not
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
    // Move the item that the player picked up into their gameObject to make it theirs and add it to the inventory
    private void InventoryItemReparented(GameObject entity, GameObject newParent)
    {
        Optional<ItemsContainer> opContainer = InventoryContainerHelper.TryGetContainerByOwner(newParent);

        if (!opContainer.HasValue)
        {
            DisplayStatusCode(StatusCode.invalidVariableVal);
            Log.Error($"Could not find container field on GameObject {newParent.GetFullHierarchyPath()}");
            return;
        }

        Pickupable pickupable = entity.RequireComponent<Pickupable>();

        ItemsContainer container = opContainer.Value;
        container.UnsafeAdd(new InventoryItem(pickupable));
    }
    // Reparent without adding it to the inventory
    private void PerformDefaultReparenting(GameObject entity, GameObject newParent)
    {
        entity.transform.SetParent(newParent.transform, false);
    }
}

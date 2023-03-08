using System;
using NitroxClient.Communication.Abstract;
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
    private readonly IPacketSender packetSender;

    public EntityReparentedProcessor(Entities entities, IPacketSender packetSender)
    {
        this.entities = entities;
        this.packetSender = packetSender;
    }

    public override void Process(EntityReparented packet)
    {
        GameObject entity = NitroxEntity.RequireObjectFrom(packet.Id);
        GameObject newParent = NitroxEntity.RequireObjectFrom(packet.NewParentId);

        using (PacketSuppressor<EntityReparented>.Suppress())
        {
            Type entityType = entities.RequireEntityType(packet.Id);

            // Move this to a resolver if there ends up being a lot of custom reparenting logic
            if (entityType == typeof(InventoryItemEntity))
            {
                InventoryItemReparented(entity, newParent);
            }
            else
            {
                PerformDefaultReparenting(entity, newParent);
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

using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic;

public class EquipmentSlots
{
    private readonly IPacketSender packetSender;
    private readonly Entities entities;

    public EquipmentSlots(IPacketSender packetSender, Entities entities)
    {
        this.packetSender = packetSender;
        this.entities = entities;
    }

    public void BroadcastEquip(Pickupable pickupable, GameObject owner, string slot)
    {
        if (!owner.TryGetIdOrWarn(out NitroxId ownerId))
        {
            return;
        }
        if (!pickupable.TryGetIdOrWarn(out NitroxId itemId))
        {
            return;
        }

        if (owner.TryGetComponent(out Player player))
        {
            entities.EntityMetadataChanged(player, ownerId);
        }
        else
        {
            // UWE also sends module events here as they are technically equipment of the vehicles.
            ModuleAdded moduleAdded = new(itemId, ownerId, slot);
            packetSender.Send(moduleAdded);
        }
    }

    public void BroadcastUnequip(Pickupable pickupable, GameObject owner, string slot)
    {
        if (!owner.TryGetIdOrWarn(out NitroxId ownerId))
        {
            return;
        }

        if (!pickupable.TryGetIdOrWarn(out NitroxId itemId))
        {
            return;
        }

        if (owner.TryGetComponent(out Player player))
        {
            entities.EntityMetadataChanged(player, ownerId);
        }
        else
        {
            // UWE also sends module events here as they are technically equipment of the vehicles.
            ModuleRemoved moduleRemoved = new(itemId, ownerId);
            packetSender.Send(moduleRemoved);
        }
    }
}

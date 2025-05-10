using System;
using NitroxModel.DataStructures.GameLogic.Entities;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record PickupItem : Packet
{
    public InventoryItemEntity Item { get; }

    public PickupItem(InventoryItemEntity item)
    {
        Item = item;
    }
}

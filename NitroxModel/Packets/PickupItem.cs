using System;
using NitroxModel.DataStructures.GameLogic.Entities;

namespace NitroxModel.Packets;

[Serializable]
public class PickupItem : Packet
{
    public InventoryItemEntity Item { get; }

    public PickupItem(InventoryItemEntity item)
    {
        Item = item;
    }
}

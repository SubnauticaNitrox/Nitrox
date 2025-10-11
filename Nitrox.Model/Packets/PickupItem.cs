using System;
using Nitrox.Model.DataStructures.GameLogic.Entities;

namespace Nitrox.Model.Packets;

[Serializable]
public class PickupItem : Packet
{
    public InventoryItemEntity Item { get; }

    public PickupItem(InventoryItemEntity item)
    {
        Item = item;
    }
}

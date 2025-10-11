using System;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class PickupItem : Packet
{
    public InventoryItemEntity Item { get; }

    public PickupItem(InventoryItemEntity item)
    {
        Item = item;
    }
}

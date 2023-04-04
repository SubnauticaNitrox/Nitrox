using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities;

namespace NitroxModel.Packets;

[Serializable]
public class PickupItem : Packet
{
    public NitroxId Id { get; }

    public InventoryItemEntity Item { get; }

    public PickupItem(NitroxId id, InventoryItemEntity item)
    {
        Id = id;
        Item = item;
    }
}

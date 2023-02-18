using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

[Serializable]
public class PlayerQuickSlotsBindingChanged : Packet
{
    public NitroxId[] SlotItemIds { get; }

    public PlayerQuickSlotsBindingChanged(NitroxId[] slotItemIds)
    {
        SlotItemIds = slotItemIds;
    }
}

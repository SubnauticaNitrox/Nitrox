using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.Packets;

[Serializable]
public class PlayerQuickSlotsBindingChanged : Packet
{
    public Optional<NitroxId>[] SlotItemIds { get; }

    public PlayerQuickSlotsBindingChanged(Optional<NitroxId>[] slotItemIds)
    {
        SlotItemIds = slotItemIds;
    }
}

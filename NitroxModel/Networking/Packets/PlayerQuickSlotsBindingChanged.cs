using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record PlayerQuickSlotsBindingChanged : Packet
{
    public Optional<NitroxId>[] SlotItemIds { get; }

    public PlayerQuickSlotsBindingChanged(Optional<NitroxId>[] slotItemIds)
    {
        SlotItemIds = slotItemIds;
    }
}

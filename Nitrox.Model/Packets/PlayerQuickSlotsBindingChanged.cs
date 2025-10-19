using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Util;

namespace Nitrox.Model.Packets;

[Serializable]
public class PlayerQuickSlotsBindingChanged : Packet
{
    public Optional<NitroxId>[] SlotItemIds { get; }

    public PlayerQuickSlotsBindingChanged(Optional<NitroxId>[] slotItemIds)
    {
        SlotItemIds = slotItemIds;
    }
}

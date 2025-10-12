using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class PlayerQuickSlotsBindingChanged : Packet
{
    public Optional<NitroxId>[] SlotItemIds { get; }

    public PlayerQuickSlotsBindingChanged(Optional<NitroxId>[] slotItemIds)
    {
        SlotItemIds = slotItemIds;
    }
}

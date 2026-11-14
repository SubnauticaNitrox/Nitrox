using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public sealed class PlayerQuickSlotsBindingChanged(Optional<NitroxId>[] slotItemIds) : Packet
{
    public Optional<NitroxId>[] SlotItemIds { get; } = slotItemIds;
}

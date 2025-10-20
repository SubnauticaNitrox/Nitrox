using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public sealed class CoffeeMachineUse : Packet
{
    public NitroxId Id { get; }
    public CoffeeMachineSlot Slot { get; }

    public CoffeeMachineUse(NitroxId id, CoffeeMachineSlot slot)
    {
        Id = id;
        Slot = slot;
    }

    public enum CoffeeMachineSlot
    {
        ONE,
        TWO
    }
}

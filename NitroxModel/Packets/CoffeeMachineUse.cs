using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

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
}

public enum CoffeeMachineSlot
{
    ONE,
    TWO
}

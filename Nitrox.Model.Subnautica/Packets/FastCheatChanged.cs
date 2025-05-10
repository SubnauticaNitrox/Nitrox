using System;
using NitroxModel.Networking.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public record FastCheatChanged(FastCheatChanged.FastCheat Cheat, bool Value) : Packet
{
    public enum FastCheat : byte
    {
        HATCH,
        GROW
    }
}

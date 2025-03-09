using System;

namespace NitroxModel.Packets;

[Serializable]
public class FastCheatChanged : Packet
{
    public FastCheat Cheat{ get; }
    public bool Value { get; }

    public FastCheatChanged(FastCheat cheat, bool value)
    {
        Cheat = cheat;
        Value = value;
    }

    public enum FastCheat : byte
    {
        HATCH,
        GROW
    }
}

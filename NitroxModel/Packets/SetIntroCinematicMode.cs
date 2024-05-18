using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets;

[Serializable]
public class SetIntroCinematicMode : Packet
{
    public ushort? PlayerId { get; }
    public IntroCinematicMode Mode { get; }

    public SetIntroCinematicMode(ushort? playerId, IntroCinematicMode mode)
    {
        PlayerId = playerId;
        Mode = mode;
    }
}

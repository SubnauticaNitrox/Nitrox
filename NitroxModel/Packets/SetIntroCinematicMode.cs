using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets;

[Serializable]
public class SetIntroCinematicMode : Packet
{
    public ushort PlayerId { get; }
    public IntroCinematicMode Mode { get; }
    public ushort? PartnerId { get; set; }

    public SetIntroCinematicMode(ushort playerId, IntroCinematicMode mode)
    {
        PlayerId = playerId;
        Mode = mode;
        PartnerId = null;
    }

    public SetIntroCinematicMode(ushort playerId, IntroCinematicMode mode, ushort? partnerId)
    {
        PlayerId = playerId;
        Mode = mode;
        PartnerId = partnerId;
    }
}

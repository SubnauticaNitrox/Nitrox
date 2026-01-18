using System;
using Nitrox.Model.Core;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class SetIntroCinematicMode : Packet
{
    public SessionId PlayerId { get; }
    public IntroCinematicMode Mode { get; }
    public ushort? PartnerId { get; set; }

    public SetIntroCinematicMode(SessionId playerId, IntroCinematicMode mode)
    {
        PlayerId = playerId;
        Mode = mode;
        PartnerId = null;
    }

    public SetIntroCinematicMode(SessionId playerId, IntroCinematicMode mode, ushort? partnerId)
    {
        PlayerId = playerId;
        Mode = mode;
        PartnerId = partnerId;
    }
}

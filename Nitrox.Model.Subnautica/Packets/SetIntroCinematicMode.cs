using System;
using Nitrox.Model.Core;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class SetIntroCinematicMode : Packet
{
    public SessionId SessionId { get; }
    public IntroCinematicMode Mode { get; }
    public SessionId? PartnerId { get; set; }

    public SetIntroCinematicMode(SessionId sessionId, IntroCinematicMode mode)
    {
        SessionId = sessionId;
        Mode = mode;
        PartnerId = null;
    }

    public SetIntroCinematicMode(SessionId sessionId, IntroCinematicMode mode, SessionId? partnerId)
    {
        SessionId = sessionId;
        Mode = mode;
        PartnerId = partnerId;
    }
}

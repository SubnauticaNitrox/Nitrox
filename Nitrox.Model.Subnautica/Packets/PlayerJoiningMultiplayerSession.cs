using System;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class PlayerJoiningMultiplayerSession : CorrelatedPacket
{
    public PlayerJoiningMultiplayerSession(string correlationId) : base(correlationId)
    {
        CorrelationId = correlationId;
    }
}

using System;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record Disconnect : Packet
{
    public Disconnect(SessionId sessionId)
    {
        SessionId = sessionId;
    }

    public SessionId SessionId { get; }
}

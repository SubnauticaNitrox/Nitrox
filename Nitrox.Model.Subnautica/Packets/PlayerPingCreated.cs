using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Packets;

namespace NitroxModel.Packets;

[Serializable]
public class PlayerPingCreated : Packet
{
    public SessionId SessionId { get; }
    public string PlayerName { get; }
    public NitroxVector3 Position { get; }
    public NitroxId PingId { get; }

    public PlayerPingCreated(SessionId sessionId, string playerName, NitroxVector3 position, NitroxId pingId)
    {
        SessionId = sessionId;
        PlayerName = playerName;
        Position = position;
        PingId = pingId;
    }
}

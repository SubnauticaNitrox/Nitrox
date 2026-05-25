using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class PlayerPingCreated(SessionId sessionId, string text, NitroxVector3 position, NitroxId pingId)
    : Packet
{
    public SessionId SessionId { get; } = sessionId;
    public string Text { get; } = text;
    public NitroxVector3 Position { get; } = position;
    public NitroxId PingId { get; } = pingId;
}

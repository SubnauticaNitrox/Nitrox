using System;
using Nitrox.Model.Core;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class Disconnect(SessionId playerId) : Packet
{
    public SessionId PlayerId { get; } = playerId;
}

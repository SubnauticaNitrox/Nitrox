using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public sealed class SubRootChanged(SessionId sessionId, Optional<NitroxId> subRootId) : Packet
{
    public SessionId SessionId { get; } = sessionId;
    public Optional<NitroxId> SubRootId { get; } = subRootId;
}

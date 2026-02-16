using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class SubRootChanged : Packet
{
    public SessionId SessionId { get; }
    public Optional<NitroxId> SubRootId { get; }

    public SubRootChanged(SessionId sessionId, Optional<NitroxId> subRootId)
    {
        SessionId = sessionId;
        SubRootId = subRootId;
    }
}

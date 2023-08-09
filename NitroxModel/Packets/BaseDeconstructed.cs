using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using System;

namespace NitroxModel.Packets;

[Serializable]
public sealed class BaseDeconstructed : Packet
{
    public NitroxId FormerBaseId { get; }
    public GhostEntity ReplacerGhost { get; }

    public BaseDeconstructed(NitroxId formerBaseId, GhostEntity replacerGhost)
    {
        FormerBaseId = formerBaseId;
        ReplacerGhost = replacerGhost;
    }
}

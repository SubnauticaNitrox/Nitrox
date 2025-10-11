using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic.Entities.Bases;

namespace Nitrox.Model.Packets;

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

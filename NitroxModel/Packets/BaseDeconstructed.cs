using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;

namespace NitroxModel.Packets;

public sealed class BaseDeconstructed : Packet
{
    public NitroxId FormerBaseId;
    public GhostEntity ReplacerGhost;

    public BaseDeconstructed(NitroxId formerBaseId, GhostEntity replacerGhost)
    {
        FormerBaseId = formerBaseId;
        ReplacerGhost = replacerGhost;
    }

    public override string ToString()
    {
        return $"BaseDeconstructed [FormerBaseId: {FormerBaseId}, ReplacerGhost: {ReplacerGhost}]";
    }
}

using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

public sealed class PlaceBase : Packet
{
    public NitroxId FormerGhostId;
    public BuildEntity BuildEntity;

    public PlaceBase(NitroxId formerGhostId, BuildEntity buildEntity)
    {
        FormerGhostId = formerGhostId;
        BuildEntity = buildEntity;
    }

    public override string ToString()
    {
        return $"PlaceBase [FormerGhostId: {FormerGhostId}, BuildEntity: {BuildEntity}]";
    }
}

using NitroxModel.DataStructures.GameLogic.Entities.Bases;

namespace NitroxModel.Packets;

public sealed class PlaceGhost : Packet
{
    public GhostEntity GhostEntity;

    public PlaceGhost(GhostEntity ghostEntity)
    {
        GhostEntity = ghostEntity;
    }

    public override string ToString()
    {
        return $"PlaceGhost [GhostEntity: {GhostEntity}]";
    }
}

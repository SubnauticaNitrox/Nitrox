using System;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;

namespace NitroxModel.Packets;

[Serializable]
public sealed class PlaceGhost : Packet
{
    public GhostEntity GhostEntity { get; }

    public PlaceGhost(GhostEntity ghostEntity)
    {
        GhostEntity = ghostEntity;
    }
}

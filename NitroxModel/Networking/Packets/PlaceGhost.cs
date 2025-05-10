using System;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;

namespace NitroxModel.Networking.Packets;

[Serializable]
public sealed record PlaceGhost : Packet
{
    public GhostEntity GhostEntity { get; }

    public PlaceGhost(GhostEntity ghostEntity)
    {
        GhostEntity = ghostEntity;
    }
}

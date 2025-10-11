using System;
using Nitrox.Model.DataStructures.GameLogic.Entities.Bases;

namespace Nitrox.Model.Packets;

[Serializable]
public sealed class PlaceGhost : Packet
{
    public GhostEntity GhostEntity { get; }

    public PlaceGhost(GhostEntity ghostEntity)
    {
        GhostEntity = ghostEntity;
    }
}

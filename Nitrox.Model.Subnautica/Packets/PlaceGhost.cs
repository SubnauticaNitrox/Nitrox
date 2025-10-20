using System;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Bases;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public sealed class PlaceGhost : Packet
{
    public GhostEntity GhostEntity { get; }

    public PlaceGhost(GhostEntity ghostEntity)
    {
        GhostEntity = ghostEntity;
    }
}

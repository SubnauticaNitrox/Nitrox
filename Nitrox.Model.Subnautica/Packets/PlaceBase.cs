using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Bases;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public sealed class PlaceBase : Packet
{
    public NitroxId FormerGhostId { get; }
    public BuildEntity BuildEntity { get; set; }

    public PlaceBase(NitroxId formerGhostId, BuildEntity buildEntity)
    {
        FormerGhostId = formerGhostId;
        BuildEntity = buildEntity;
    }

    public void Deflate()
    {
        BuildEntity = null;
    }
}

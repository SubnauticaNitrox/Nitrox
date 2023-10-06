using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;

namespace NitroxModel.Packets;

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

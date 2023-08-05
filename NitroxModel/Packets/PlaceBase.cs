using System;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures;

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
}

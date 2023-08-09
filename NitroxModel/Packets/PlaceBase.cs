using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using System;

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

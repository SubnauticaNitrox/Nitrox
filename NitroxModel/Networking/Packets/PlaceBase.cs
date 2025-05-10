using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;

namespace NitroxModel.Networking.Packets;

[Serializable]
public sealed record PlaceBase : Packet
{
    public PlaceBase(NitroxId formerGhostId, BuildEntity buildEntity)
    {
        FormerGhostId = formerGhostId;
        BuildEntity = buildEntity;
    }

    public NitroxId FormerGhostId { get; }
    public BuildEntity BuildEntity { get; set; }

    /// <summary>
    ///     End-players can process elementary operations without this data (packet would be heavier for no reason).
    /// </summary>
    public void Deflate() => BuildEntity = null;
}

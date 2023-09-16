using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Bases;

namespace NitroxModel.Packets;

[Serializable]
public sealed class UpdateBase : OrderedBuildPacket
{
    public NitroxId BaseId { get; }
    public NitroxId FormerGhostId { get; }
    public BaseData BaseData { get; set;  }
    public Entity BuiltPieceEntity { get; set; }
    public Dictionary<NitroxId, NitroxBaseFace> UpdatedChildren { get; set; }
    public Dictionary<NitroxId, NitroxInt3> UpdatedMoonpools { get; set; }
    public Dictionary<NitroxId, NitroxInt3> UpdatedMapRooms { get; set; }
    public (NitroxId, NitroxId) ChildrenTransfer { get; }

    public UpdateBase(NitroxId baseId, NitroxId formerGhostId, BaseData baseData, Entity builtPieceEntity,
                      Dictionary<NitroxId, NitroxBaseFace> updatedChildren, Dictionary<NitroxId, NitroxInt3> updatedMoonpools,
                      Dictionary<NitroxId, NitroxInt3> updatedMapRooms, (NitroxId, NitroxId) childrenTransfer, int operationId) : base(operationId)
    {
        BaseId = baseId;
        FormerGhostId = formerGhostId;
        BaseData = baseData;
        BuiltPieceEntity = builtPieceEntity;
        UpdatedChildren = updatedChildren;
        UpdatedMoonpools = updatedMoonpools;
        UpdatedMapRooms = updatedMapRooms;
        ChildrenTransfer = childrenTransfer;
    }

    public void Deflate()
    {
        BaseData = null;
        BuiltPieceEntity = null;
        UpdatedChildren = null;
        UpdatedMoonpools = null;
        UpdatedMapRooms = null;
    }
}

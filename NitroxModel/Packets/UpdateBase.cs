using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Bases;
using System.Collections.Generic;

namespace NitroxModel.Packets;

public sealed class UpdateBase : OrderedBuildPacket
{
    public NitroxId BaseId;
    public NitroxId FormerGhostId;
    public SavedBase SavedBase;
    public Entity BuiltPieceEntity;
    public Dictionary<NitroxId, NitroxBaseFace> UpdatedChildren;
    public Dictionary<NitroxId, NitroxInt3> UpdatedMoonpools;
    public Dictionary<NitroxId, NitroxInt3> UpdatedMapRooms;
    public (NitroxId, NitroxId) ChildrenTransfer;

    public UpdateBase(NitroxId baseId, NitroxId formerGhostId, SavedBase savedBase, Entity builtPieceEntity, Dictionary<NitroxId, NitroxBaseFace> updatedChildren, Dictionary<NitroxId, NitroxInt3> updatedMoonpools, Dictionary<NitroxId, NitroxInt3> updatedMapRooms, (NitroxId, NitroxId) childrenTransfer, int operationId) : base(operationId)
    {
        BaseId = baseId;
        FormerGhostId = formerGhostId;
        SavedBase = savedBase;
        BuiltPieceEntity = builtPieceEntity;
        UpdatedChildren = updatedChildren;
        UpdatedMoonpools = updatedMoonpools;
        UpdatedMapRooms = updatedMapRooms;
        ChildrenTransfer = childrenTransfer;
    }

    public override string ToString()
    {
        return $"UpdateBase [{OperationId}] [BaseId: {BaseId}, FormerGhostId: {FormerGhostId}, SavedBase: {SavedBase}, BuiltPieceEntity: {BuiltPieceEntity}, UpdatedChildren: {UpdatedChildren.Count}, UpdatedMoonpools: {UpdatedMoonpools.Count}, UpdatedMapRooms: {UpdatedMapRooms.Count}, ChildrenTransfer: {ChildrenTransfer}]";
    }
}

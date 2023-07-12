using NitroxModel.DataStructures.GameLogic.Bases;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures;
using System.Collections.Generic;

namespace NitroxModel.Packets;

public sealed class WaterParkDeconstructed : PieceDeconstructed
{
    public InteriorPieceEntity NewWaterPark;
    public List<NitroxId> MovedChildrenIds;
    public bool Transfer;

    public WaterParkDeconstructed(NitroxId baseId, NitroxId pieceId, BuildPieceIdentifier buildPieceIdentifier, GhostEntity replacerGhost, SavedBase savedBase, InteriorPieceEntity newWaterPark, List<NitroxId> movedChildrenIds, bool transfer, int operationId) : base(baseId, pieceId, buildPieceIdentifier, replacerGhost, savedBase, operationId)
    {
        NewWaterPark = newWaterPark;
        MovedChildrenIds = movedChildrenIds ?? new();
        Transfer = transfer;
    }

    public override string ToString()
    {
        return $"WaterParkDeconstructed [{base.ToString()}, NewWaterPark: {NewWaterPark}, MovedChildrenIds: {MovedChildrenIds?.Count}, Transfer: {Transfer}]";
    }
}

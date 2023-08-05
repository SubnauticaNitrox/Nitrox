using System;
using NitroxModel.DataStructures.GameLogic.Bases;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures;
using System.Collections.Generic;

namespace NitroxModel.Packets;

[Serializable]
public sealed class WaterParkDeconstructed : PieceDeconstructed
{
    public InteriorPieceEntity NewWaterPark { get; }
    public List<NitroxId> MovedChildrenIds { get; }
    public bool Transfer { get; }

    public WaterParkDeconstructed(NitroxId baseId, NitroxId pieceId, BuildPieceIdentifier buildPieceIdentifier, GhostEntity replacerGhost, BaseData baseData, InteriorPieceEntity newWaterPark, List<NitroxId> movedChildrenIds, bool transfer, int operationId) : base(baseId, pieceId, buildPieceIdentifier, replacerGhost, baseData, operationId)
    {
        NewWaterPark = newWaterPark;
        MovedChildrenIds = movedChildrenIds ?? new List<NitroxId>();
        Transfer = transfer;
    }
}

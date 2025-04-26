using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Bases;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;

namespace NitroxModel.Packets;

[Serializable]
public sealed class LargeWaterParkDeconstructed : PieceDeconstructed
{
    public Dictionary<NitroxId, List<NitroxId>> MovedChildrenIdsByNewHostId;

    public LargeWaterParkDeconstructed(NitroxId baseId, NitroxId pieceId, BuildPieceIdentifier buildPieceIdentifier, GhostEntity replacerGhost, BaseData baseData, Dictionary<NitroxId, List<NitroxId>> movedChildrenIdsByNewHostId, int operationId) :
        base(baseId, pieceId, buildPieceIdentifier, replacerGhost, baseData, operationId)
    {
        MovedChildrenIdsByNewHostId = movedChildrenIdsByNewHostId;
    }
}

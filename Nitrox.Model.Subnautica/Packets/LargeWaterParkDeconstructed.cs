using System;
using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Bases;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Bases;

namespace Nitrox.Model.Subnautica.Packets;

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

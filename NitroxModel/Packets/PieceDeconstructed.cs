using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Bases;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;

namespace NitroxModel.Packets;

[Serializable]
public class PieceDeconstructed : OrderedBuildPacket
{
    public NitroxId BaseId { get; }
    public NitroxId PieceId { get; }
    public BuildPieceIdentifier BuildPieceIdentifier { get; }
    public GhostEntity ReplacerGhost { get; }
    public BaseData BaseData { get; set;  }

    public PieceDeconstructed(NitroxId baseId, NitroxId pieceId, BuildPieceIdentifier buildPieceIdentifier, GhostEntity replacerGhost, BaseData baseData, int operationId) : base(operationId)
    {
        BaseId = baseId;
        PieceId = pieceId;
        BuildPieceIdentifier = buildPieceIdentifier;
        ReplacerGhost = replacerGhost;
        BaseData = baseData;
    }
}

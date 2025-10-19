using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic.Bases;
using Nitrox.Model.DataStructures.GameLogic.Entities.Bases;

namespace Nitrox.Model.Packets;

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

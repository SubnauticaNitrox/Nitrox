using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Bases;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;

namespace NitroxModel.Packets;

public class PieceDeconstructed : OrderedBuildPacket
{
    public NitroxId BaseId;
    public NitroxId PieceId;
    public BuildPieceIdentifier BuildPieceIdentifier;
    public GhostEntity ReplacerGhost;
    public BaseData BaseData;

    public PieceDeconstructed(NitroxId baseId, NitroxId pieceId, BuildPieceIdentifier buildPieceIdentifier, GhostEntity replacerGhost, BaseData baseData, int operationId) : base(operationId)
    {
        BaseId = baseId;
        PieceId = pieceId;
        BuildPieceIdentifier = buildPieceIdentifier;
        ReplacerGhost = replacerGhost;
        BaseData = baseData;
    }

    public override string ToString()
    {
        return $"PieceDeconstructed [{OperationId}] [BaseId: {BaseId}, PieceId: {PieceId}, PieceIdentifier: {BuildPieceIdentifier}, ReplacerGhost: {ReplacerGhost}]";
    }
}

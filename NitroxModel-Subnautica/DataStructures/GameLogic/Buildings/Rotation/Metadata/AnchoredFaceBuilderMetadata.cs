using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.Serialization;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation.Metadata;

[Serializable]
[JsonContractTransition]
public class AnchoredFaceBuilderMetadata : BuilderMetadata
{
    [JsonMemberTransition]
    public NitroxInt3 Cell { get; set; }

    [JsonMemberTransition]
    public int Direction { get; set; }

    [JsonMemberTransition]
    public int FaceType { get; set; }

    public AnchoredFaceBuilderMetadata(NitroxInt3 cell, int direction, int faceType) : base(typeof(BaseAddFaceGhost))
    {
        Cell = cell;
        Direction = direction;
        FaceType = faceType;
    }

    public override string ToString()
    {
        return $"[AnchoredFaceRotationMetadata - Cell: {Cell}, Direction: {Direction}, FaceType: {FaceType}]";
    }
}

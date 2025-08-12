using System;
using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Bases;

[Serializable, DataContract]
public record struct BuildPieceIdentifier(NitroxTechType Recipe, NitroxBaseFace? BaseFace, NitroxInt3 BaseCell, NitroxInt3 PiecePoint)
{
    [DataMember(Order = 1)]
    public NitroxTechType Recipe = Recipe;

    [DataMember(Order = 2)]
    public NitroxBaseFace? BaseFace = BaseFace;

    [DataMember(Order = 3)]
    public NitroxInt3 BaseCell = BaseCell;

    [DataMember(Order = 4)]
    public NitroxInt3 PiecePoint = PiecePoint;

    public override readonly string ToString()
    {
        return $"BuildPieceIdentifier (Recipe: {Recipe}, BaseFace: {BaseFace}, BaseCell: {BaseCell}, PiecePoint: {PiecePoint})";
    }
}

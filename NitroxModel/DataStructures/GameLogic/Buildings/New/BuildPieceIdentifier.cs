namespace NitroxModel.DataStructures.GameLogic.Buildings.New;

public class BuildPieceIdentifier
{
    public NitroxTechType Recipe;
    public NitroxBaseFace? BaseFace;
    public int FaceType;
    public NitroxInt3 BaseCell;

    public BuildPieceIdentifier(NitroxTechType recipe, NitroxBaseFace? baseFace, int faceType, NitroxInt3 baseCell)
    {
        Recipe = recipe;
        BaseFace = baseFace;
        FaceType = faceType;
        BaseCell = baseCell;
    }

    public override string ToString()
    {
        return $"BuildPieceIdentifier [Recipe: {Recipe}, BaseFace: {BaseFace}, FaceType: {FaceType}, BaseCell: {BaseCell}]";
    }
}

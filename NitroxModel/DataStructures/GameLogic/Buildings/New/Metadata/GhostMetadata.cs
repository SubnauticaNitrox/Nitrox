namespace NitroxModel.DataStructures.GameLogic.Buildings.New.Metadata;

public class GhostMetadata
{
    public NitroxInt3 TargetOffset;
}

public class BaseAnchoredFaceGhostMetadata : GhostMetadata
{
    public NitroxBaseFace? AnchoredFace;
}

public class BaseAnchoredCellGhostMetadata : GhostMetadata
{
    public NitroxInt3? AnchoredCell;
}

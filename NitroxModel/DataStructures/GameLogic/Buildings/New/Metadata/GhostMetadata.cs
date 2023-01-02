using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Buildings.New.Metadata;

[DataContract]
public abstract class GhostMetadata
{
    [DataMember(Order = 1)]
    public NitroxInt3 TargetOffset;
}

[DataContract]
public class BasicGhostMetadata : GhostMetadata { }

[DataContract]
public class BaseAnchoredFaceGhostMetadata : GhostMetadata
{
    [DataMember(Order = 1)]
    public NitroxBaseFace? AnchoredFace;
}

[DataContract]
public class BaseAnchoredCellGhostMetadata : GhostMetadata
{
    [DataMember(Order = 1)]
    public NitroxInt3? AnchoredCell;
}

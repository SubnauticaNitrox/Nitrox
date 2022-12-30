using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Buildings.New.Metadata;

[ProtoContract]
public abstract class GhostMetadata
{
    [ProtoMember(1)]
    public NitroxInt3 TargetOffset;
}

[ProtoContract]
public class BasicGhostMetadata : GhostMetadata { }

[ProtoContract]
public class BaseAnchoredFaceGhostMetadata : GhostMetadata
{
    [ProtoMember(1)]
    public NitroxBaseFace? AnchoredFace;
}

[ProtoContract]
public class BaseAnchoredCellGhostMetadata : GhostMetadata
{
    [ProtoMember(1)]
    public NitroxInt3? AnchoredCell;
}

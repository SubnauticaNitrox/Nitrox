using System;
using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Buildings.New.Metadata;

[Serializable, DataContract]
public abstract class GhostMetadata
{
    [DataMember(Order = 1)]
    public NitroxInt3 TargetOffset;
}

[Serializable, DataContract]
public class BasicGhostMetadata : GhostMetadata { }

[Serializable, DataContract]
public class BaseDeconstructableGhostMetadata : GhostMetadata
{
    [DataMember(Order = 1)]
    public NitroxBaseFace? ModuleFace;

    [DataMember(Order = 2)]
    public string ClassId;
}

[Serializable, DataContract]
public class BaseAnchoredFaceGhostMetadata : GhostMetadata
{
    [DataMember(Order = 1)]
    public NitroxBaseFace? AnchoredFace;
}

[Serializable, DataContract]
public class BaseAnchoredCellGhostMetadata : GhostMetadata
{
    [DataMember(Order = 1)]
    public NitroxInt3? AnchoredCell;
}

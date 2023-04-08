using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using ProtoBufNet;
using System;
using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Buildings.New.Metadata;

[ProtoInclude(271, typeof(BasicGhostMetadata))]
[ProtoInclude(272, typeof(BaseDeconstructableGhostMetadata))]
[ProtoInclude(273, typeof(BaseAnchoredFaceGhostMetadata))]
[ProtoInclude(274, typeof(BaseAnchoredCellGhostMetadata))]
[Serializable, DataContract]
public abstract class GhostMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public NitroxInt3 TargetOffset;

    [IgnoreConstructor]
    protected GhostMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }
}

[Serializable, DataContract]
public class BasicGhostMetadata : GhostMetadata
{
    public BasicGhostMetadata()
    {
        // Constructor to be able to use this type as T
    }

    public BasicGhostMetadata(NitroxInt3 targetOffset)
    {
        TargetOffset = targetOffset;
    }
}

[Serializable, DataContract]
public class BaseDeconstructableGhostMetadata : GhostMetadata
{
    [DataMember(Order = 1)]
    public NitroxBaseFace? ModuleFace;

    [DataMember(Order = 2)]
    public string ClassId;

    public BaseDeconstructableGhostMetadata()
    {
        // Constructor to be able to use this type as T
    }

    public BaseDeconstructableGhostMetadata(NitroxInt3 targetOffset, NitroxBaseFace? moduleFace, string classId)
    {
        TargetOffset = targetOffset;
        ModuleFace = moduleFace;
        ClassId = classId;
    }
}

[Serializable, DataContract]
public class BaseAnchoredFaceGhostMetadata : GhostMetadata
{
    [DataMember(Order = 1)]
    public NitroxBaseFace? AnchoredFace;

    public BaseAnchoredFaceGhostMetadata()
    {
        // Constructor to be able to use this type as T
    }

    public BaseAnchoredFaceGhostMetadata(NitroxInt3 targetOffset, NitroxBaseFace? anchoredFace)
    {
        TargetOffset = targetOffset;
        AnchoredFace = anchoredFace;
    }
}

[Serializable, DataContract]
public class BaseAnchoredCellGhostMetadata : GhostMetadata
{
    [DataMember(Order = 1)]
    public NitroxInt3? AnchoredCell;

    public BaseAnchoredCellGhostMetadata()
    {
        // Constructor to be able to use this type as T
    }

    public BaseAnchoredCellGhostMetadata(NitroxInt3 targetOffset, NitroxInt3? anchoredCell)
    {
        TargetOffset = targetOffset;
        AnchoredCell = anchoredCell;
    }
}

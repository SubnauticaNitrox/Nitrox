using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using ProtoBufNet;
using System;
using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Bases.Metadata;

[ProtoInclude(281, typeof(BasicGhostMetadata))]
[ProtoInclude(282, typeof(BaseDeconstructableGhostMetadata))]
[ProtoInclude(283, typeof(BaseAnchoredFaceGhostMetadata))]
[ProtoInclude(284, typeof(BaseAnchoredCellGhostMetadata))]
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

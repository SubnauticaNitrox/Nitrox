using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata.Bases;

[Serializable, DataContract]
[ProtoInclude(50, typeof(BaseDeconstructableGhostMetadata))]
[ProtoInclude(51, typeof(BaseAnchoredFaceGhostMetadata))]
[ProtoInclude(52, typeof(BaseAnchoredCellGhostMetadata))]
public class GhostMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public NitroxInt3 TargetOffset { get; set; }

    [IgnoreConstructor]
    public GhostMetadata()
    {
        // Constructor for ProtoBuf deserialization.
    }

    /// <remarks>Used for json deserialization</remarks>
    public GhostMetadata(NitroxInt3 targetOffset)
    {
        TargetOffset = targetOffset;
    }
}

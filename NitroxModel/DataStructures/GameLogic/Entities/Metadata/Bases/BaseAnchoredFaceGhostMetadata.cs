using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Bases;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata.Bases;

[Serializable, DataContract]
public class BaseAnchoredFaceGhostMetadata : GhostMetadata
{
    [DataMember(Order = 1)]
    public NitroxBaseFace? AnchoredFace { get; set; }

    [IgnoreConstructor]
    public BaseAnchoredFaceGhostMetadata()
    {
        // Constructor for ProtoBuf deserialization.
    }

    /// <remarks>Used for json deserialization</remarks>
    public BaseAnchoredFaceGhostMetadata(NitroxBaseFace? anchoredFace, NitroxInt3 targetOffset) : base(targetOffset)
    {
        AnchoredFace = anchoredFace;
    }
}

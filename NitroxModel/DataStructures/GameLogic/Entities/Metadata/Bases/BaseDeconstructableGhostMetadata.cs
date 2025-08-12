using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Bases;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata.Bases;

[Serializable, DataContract]
public class BaseDeconstructableGhostMetadata : GhostMetadata
{
    [DataMember(Order = 1)]
    public NitroxBaseFace? ModuleFace { get; set; }

    [DataMember(Order = 2)]
    public string ClassId { get; set; }

    [IgnoreConstructor]
    public BaseDeconstructableGhostMetadata()
    {
        // Constructor for ProtoBuf deserialization.
    }

    /// <remarks>Used for json deserialization</remarks>
    public BaseDeconstructableGhostMetadata(NitroxBaseFace? moduleFace, string classId, NitroxInt3 targetOffset) : base(targetOffset)
    {
        ModuleFace = moduleFace;
        ClassId = classId;
    }

    public override string ToString()
    {
        return $"[BaseDeconstructableGhostMetadata TargetOffset: {TargetOffset}, ModuleFace: {ModuleFace}, ClassId: {ClassId}]";
    }
}

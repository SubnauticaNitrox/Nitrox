using System;
using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Bases.Metadata;

[Serializable, DataContract]
public class BaseDeconstructableGhostMetadata : GhostMetadata
{
    [DataMember(Order = 1)]
    public NitroxBaseFace? ModuleFace;

    [DataMember(Order = 2)]
    public string ClassId;

    public BaseDeconstructableGhostMetadata()
    {
        // Constructor to be able to use this type as a generic type
    }

    public BaseDeconstructableGhostMetadata(NitroxInt3 targetOffset, NitroxBaseFace? moduleFace, string classId)
    {
        TargetOffset = targetOffset;
        ModuleFace = moduleFace;
        ClassId = classId;
    }

    public override string ToString()
    {
        return $"[BaseDeconstructableGhostMetadata TargetOffset: {TargetOffset}, ModuleFace: {ModuleFace}, ClassId: {ClassId}]";
    }
}

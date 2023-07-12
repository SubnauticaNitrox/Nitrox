using System;
using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Bases.Metadata;

[Serializable, DataContract]
public class BaseAnchoredFaceGhostMetadata : GhostMetadata
{
    [DataMember(Order = 1)]
    public NitroxBaseFace? AnchoredFace;

    public BaseAnchoredFaceGhostMetadata()
    {
        // Constructor to be able to use this type as a generic type
    }

    public BaseAnchoredFaceGhostMetadata(NitroxInt3 targetOffset, NitroxBaseFace? anchoredFace)
    {
        TargetOffset = targetOffset;
        AnchoredFace = anchoredFace;
    }
}

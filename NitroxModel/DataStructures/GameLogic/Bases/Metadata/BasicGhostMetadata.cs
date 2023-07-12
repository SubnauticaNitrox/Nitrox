using System;
using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Bases.Metadata;

[Serializable, DataContract]
public class BasicGhostMetadata : GhostMetadata
{
    public BasicGhostMetadata()
    {
        // Constructor to be able to use this type as a generic type
    }

    public BasicGhostMetadata(NitroxInt3 targetOffset)
    {
        TargetOffset = targetOffset;
    }
}

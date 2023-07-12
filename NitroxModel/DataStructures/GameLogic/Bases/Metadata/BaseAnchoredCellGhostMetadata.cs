using System;
using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Bases.Metadata;

[Serializable, DataContract]
public class BaseAnchoredCellGhostMetadata : GhostMetadata
{
    [DataMember(Order = 1)]
    public NitroxInt3? AnchoredCell;

    public BaseAnchoredCellGhostMetadata()
    {
        // Constructor to be able to use this type as a generic type
    }

    public BaseAnchoredCellGhostMetadata(NitroxInt3 targetOffset, NitroxInt3? anchoredCell)
    {
        TargetOffset = targetOffset;
        AnchoredCell = anchoredCell;
    }
}

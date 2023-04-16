using BinaryPack.Attributes;
using System;
using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable, DataContract]
public abstract class InteriorPieceMetadata : EntityMetadata
{
    [IgnoreConstructor]
    protected InteriorPieceMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }
}

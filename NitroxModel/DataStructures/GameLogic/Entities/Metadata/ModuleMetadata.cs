using BinaryPack.Attributes;
using System;
using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable, DataContract]
public abstract class ModuleMetadata : EntityMetadata
{
    [IgnoreConstructor]
    protected ModuleMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }
}

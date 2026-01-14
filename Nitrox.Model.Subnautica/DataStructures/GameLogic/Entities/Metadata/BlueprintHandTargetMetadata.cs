using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class BlueprintHandTargetMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public bool Used { get; }

    [IgnoreConstructor]
    protected BlueprintHandTargetMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public BlueprintHandTargetMetadata(bool used)
    {
        Used = used;
    }

    public override string ToString()
    {
        return $"[BlueprintHandTargetMetadata Used: {Used}]";
    }
}

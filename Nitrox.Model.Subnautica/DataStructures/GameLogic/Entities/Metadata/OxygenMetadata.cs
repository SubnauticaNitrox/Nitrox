using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class OxygenMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public float OxygenAvailable { get; }

    [IgnoreConstructor]
    protected OxygenMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public OxygenMetadata(float oxygenAvailable)
    {
        OxygenAvailable = oxygenAvailable;
    }

    public override string ToString()
    {
        return $"[{nameof(OxygenMetadata)} {nameof(OxygenAvailable)}: {OxygenAvailable}]";
    }
}

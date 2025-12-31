using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class PrecursorComputerTerminalMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public bool Used { get; }

    [IgnoreConstructor]
    protected PrecursorComputerTerminalMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public PrecursorComputerTerminalMetadata(bool used)
    {
        Used = used;
    }

    public override string ToString()
    {
        return $"[PrecursorComputerTerminalMetadata Used: {Used}]";
    }
}

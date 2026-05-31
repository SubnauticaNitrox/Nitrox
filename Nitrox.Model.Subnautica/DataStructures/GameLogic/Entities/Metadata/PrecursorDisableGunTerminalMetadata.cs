using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class PrecursorDisableGunTerminalMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public bool FirstUse { get; }

    [IgnoreConstructor]
    protected PrecursorDisableGunTerminalMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public PrecursorDisableGunTerminalMetadata(bool firstUse)
    {
        FirstUse = firstUse;
    }

    public override string ToString()
    {
        return $"[PrecursorDisableGunTerminalMetadata FirstUse: {FirstUse}]";
    }
}

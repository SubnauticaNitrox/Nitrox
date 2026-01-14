using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class GenericConsoleMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public bool GotUsed { get; }

    [IgnoreConstructor]
    protected GenericConsoleMetadata()
    {
    }

    public GenericConsoleMetadata(bool gotUsed)
    {
        GotUsed = gotUsed;
    }

    public override string ToString()
    {
        return $"[GenericConsoleMetadata GotUsed: {GotUsed}]";
    }
}

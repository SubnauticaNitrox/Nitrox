using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class SeamothMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public bool LightsOn { get; }

    [IgnoreConstructor]
    protected SeamothMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public SeamothMetadata(bool lightsOn)
    {
        LightsOn = lightsOn;
    }

    public override string ToString()
    {
        return $"[SeamothMetadata LightsOn: {LightsOn}]";
    }
}

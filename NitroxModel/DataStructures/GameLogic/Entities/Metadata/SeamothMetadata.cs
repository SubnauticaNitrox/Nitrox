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

    [DataMember(Order = 2)]
    public float Health { get; }

    [IgnoreConstructor]
    protected SeamothMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public SeamothMetadata(bool lightsOn, float health)
    {
        LightsOn = lightsOn;
        Health = health;
    }

    public override string ToString()
    {
        return $"[SeamothMetadata LightsOn: {LightsOn}, Health: {Health}]";
    }
}

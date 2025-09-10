using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class SeamothMetadata : VehicleMetadata
{
    [DataMember(Order = 1)]
    public bool LightsOn { get; }

    [IgnoreConstructor]
    protected SeamothMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public SeamothMetadata(bool lightsOn, float health, bool inPrecursor, string name, NitroxVector3[] colors) : base(health, inPrecursor, name, colors)
    {
        LightsOn = lightsOn;
    }

    public override string ToString()
    {
        return $"[{nameof(SeamothMetadata)} LightsOn: {LightsOn} {base.ToString()}]";
    }
}

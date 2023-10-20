using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable, DataContract]
public class FlareMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public float EnergyLeft { get; }

    [DataMember(Order = 2)]
    public bool HasBeenThrown { get; }

    [DataMember(Order = 3)]
    public float? FlareActivateTime { get; }

    [IgnoreConstructor]
    protected FlareMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public FlareMetadata(float energyLeft, bool hasBeenThrown, float? flareActivateTime)
    {
        EnergyLeft = energyLeft;
        HasBeenThrown = hasBeenThrown;
        FlareActivateTime = flareActivateTime;
    }

    public override string ToString()
    {
        return $"[FlareMetadata EnergyLeft: {EnergyLeft}, HasBeenThrown: {HasBeenThrown}, FlareActivateTime: {FlareActivateTime}]";
    }
}

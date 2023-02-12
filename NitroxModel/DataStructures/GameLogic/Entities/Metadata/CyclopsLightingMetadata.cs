using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class CyclopsLightingMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public bool FloodLightsOn { get; set; }

    [DataMember(Order = 2)]
    public bool InternalLightsOn { get; set; }

    [IgnoreConstructor]
    protected CyclopsLightingMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public CyclopsLightingMetadata(bool floodLightsOn, bool internalLightsOn)
    {
        FloodLightsOn = floodLightsOn;
        InternalLightsOn = internalLightsOn;
    }

    public override string ToString()
    {
        return $"[CyclopsLightningMetadata FloodLightsOn: {FloodLightsOn}, InternalLightsOn: {InternalLightsOn}]";
    }
}

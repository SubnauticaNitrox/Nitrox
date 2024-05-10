using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class PlantableMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public float TimeStartGrowth { get; }

    [DataMember(Order = 2)]
    public int SlotID { get; }

    [IgnoreConstructor]
    protected PlantableMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public PlantableMetadata(float timeStartGrowth, int slotID)
    {
        TimeStartGrowth = timeStartGrowth;
        SlotID = slotID;
    }

    public override string ToString()
    {
        return $"[{nameof(PlantableMetadata)} TimeStartGrowth: {TimeStartGrowth}, SlotID: {SlotID}]";
    }
}

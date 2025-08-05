using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class FruitPlantMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public bool[] PickedStates { get; } = [];

    [DataMember(Order = 2)]
    public float TimeNextFruit { get; }

    [IgnoreConstructor]
    protected FruitPlantMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public FruitPlantMetadata(bool[] pickedStates, float timeNextFruit)
    {
        PickedStates = pickedStates;
        TimeNextFruit = timeNextFruit;
    }

    public override string ToString()
    {
        return $"[{nameof(FruitPlantMetadata)} PickedStates: [{string.Join(", ", PickedStates)}], TimeNextFruit: {TimeNextFruit}]";
    }
}

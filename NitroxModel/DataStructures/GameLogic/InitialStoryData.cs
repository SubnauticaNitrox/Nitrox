using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic;

[Serializable, DataContract]
public class InitialStoryData
{
    [DataMember(Order = 1)]
    public InitialTimeData InitialTimeData { get; set; }

    [IgnoreConstructor]
    protected InitialStoryData()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public InitialStoryData(InitialTimeData initialTimeData)
    {
        InitialTimeData = initialTimeData;
    }
}

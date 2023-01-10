using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic;

/// <summary>
/// Sunbeam events data
/// </summary>
[Serializable, DataContract]
public class SunbeamData
{
    /// <summary>
    /// In-game time in seconds at which Sunbeam's countdown start
    /// </summary>
    [DataMember(Order = 1)]
    public float CountdownStartingTime;

    [IgnoreConstructor]
    protected SunbeamData()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public SunbeamData(float countdownStartingTime)
    {
        CountdownStartingTime = countdownStartingTime;
    }
}

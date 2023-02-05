using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic;

/// <summary>
/// Aurora events data
/// </summary>
[Serializable, DataContract]
public class CrashedShipExploderData
{
    /// <summary>
    /// In-game time in seconds at which Aurora's countdown happens
    /// </summary>
    [DataMember(Order = 1)]
    public float TimeToStartCountdown;

    /// <summary>
    /// In-game time in seconds at which Aurora's warning messages start
    /// </summary>
    [DataMember(Order = 2)]
    public float TimeToStartWarning;

    [IgnoreConstructor]
    protected CrashedShipExploderData()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public CrashedShipExploderData(float timeToStartCountdown, float timeToStartWarning)
    {
        TimeToStartCountdown = timeToStartCountdown;
        TimeToStartWarning = timeToStartWarning;
    }

    [NonSerialized]
    public static readonly IReadOnlyCollection<string> AuroraEvents = new[] { "Story_AuroraWarning1", "Story_AuroraWarning2", "Story_AuroraWarning3", "Story_AuroraWarning4", "Story_AuroraExplosion" };
}

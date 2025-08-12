using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic;

/// <summary>
/// Aurora events data
/// </summary>
[Serializable, DataContract]
public class AuroraEventData
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

    /// <summary>
    /// Real time in seconds at which Aurora's considered exploded
    /// </summary>
    [DataMember(Order = 3)]
    public float AuroraRealExplosionTime;

    [IgnoreConstructor]
    protected AuroraEventData()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public AuroraEventData(float timeToStartCountdown, float timeToStartWarning, float auroraRealExplosionTime)
    {
        TimeToStartCountdown = timeToStartCountdown;
        TimeToStartWarning = timeToStartWarning;
        AuroraRealExplosionTime = auroraRealExplosionTime;
    }

    [NonSerialized]
    public static readonly IReadOnlyCollection<string> GoalNames = new[] { "Story_AuroraWarning1", "Story_AuroraWarning2", "Story_AuroraWarning3", "Story_AuroraWarning4", "Story_AuroraExplosion" };
}

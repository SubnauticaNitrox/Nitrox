using System;
using System.Runtime.Serialization;

namespace Nitrox.Server.Subnautica.Models.Persistence;

[Serializable]
[DataContract]
internal record StoryTimingData
{
    /// <summary>
    ///     Latest registered time without taking the current stopwatch time in account.
    /// </summary>
    [DataMember(Order = 1)]
    public TimeSpan Elapsed { get; set; }

    /// <summary>
    ///     Time at which the Aurora explosion countdown will start (last warning is sent).
    /// </summary>
    /// <remarks>
    ///     It is required to calculate the time at which the Aurora warnings will be sent (along with
    ///     <see cref="AuroraWarningStartTime" />, look into AuroraWarnings.cs and CrashedShipExploder.cs for more information).
    /// </remarks>
    [DataMember(Order = 2)]
    public TimeSpan AuroraCountdownStartTime { get; set; }

    /// <summary>
    ///     Time at which the Aurora Events start (you start receiving warnings).
    /// </summary>
    [DataMember(Order = 3)]
    public TimeSpan AuroraWarningStartTime { get; set; }

    [DataMember(Order = 4)]
    public TimeSpan RealTimeElapsed { get; set; }

    /// <summary>
    ///     In seconds.
    /// </summary>
    [DataMember(Order = 5)]
    public TimeSpan AuroraRealExplosionTime { get; set; }
}

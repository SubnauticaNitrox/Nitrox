using System;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic;

public class TimeManager
{
    /// <summary>
    ///     When first player connects to the server, time will resume when time will be resumed on server-side.
    ///     According to this, we need to freeze time on first player connecting before it has fully loaded.
    /// </summary>
    private bool freezeTime = true;

    /// <summary>
    ///     Latest moment at which we updated the time
    /// </summary>
    private DateTimeOffset latestRegistrationTime;
    /// <summary>
    ///     Latest registered value of the time
    /// </summary>
    private double latestRegisteredTime;

    /// <summary>
    ///     Moment at which real time elapsed was determined
    /// </summary>
    private DateTimeOffset realTimeElapsedRegistrationTime;
    /// <summary>
    ///     Only registered value of real time elapsed given when connecting. Associated to <see cref="realTimeElapsedRegistrationTime"/>
    /// </summary>
    private double realTimeElapsed;

    public float AuroraRealExplosionTime { get; set; }

    private const double DEFAULT_REAL_TIME = 0;

    /// <summary>
    ///     Calculates the exact real time elapsed from an offset (<see cref="realTimeElapsedRegistrationTime"/>) and the delta time between
    ///     <see cref="DateTimeOffset.UtcNow"/> and the offset's exact <see cref="DateTimeOffset"/> (<see cref="latestRegistrationTime"/>).
    /// </summary>
    public double RealTimeElapsed
    {
        get
        {
            // Unitialized state
            if (realTimeElapsedRegistrationTime == default)
            {
                return DEFAULT_REAL_TIME;
            }
            if (freezeTime)
            {
                return realTimeElapsed;
            }

            return (DateTimeOffset.UtcNow - realTimeElapsedRegistrationTime).TotalMilliseconds * 0.001 + realTimeElapsed;
        }
    }

    private const double DEFAULT_SUBNAUTICA_TIME = 480;

    /// <summary>
    ///     Calculates the current exact time from an offset (<see cref="latestRegisteredTime"/>) and the delta time between
    ///     <see cref="DateTimeOffset.UtcNow"/> and the offset's exact <see cref="DateTimeOffset"/> (<see cref="latestRegistrationTime"/>).
    /// </summary>
    /// <remarks>
    ///     This should only be used for DayNigthCycle internal calculations so that we don't use different times during the same frame.
    ///     Use <see cref="DayNightCycle.timePassed"/> instead to get the current frame's time.
    /// </remarks>
    public double CurrentTime
    {
        get
        {
            // Unitialized state
            if (latestRegisteredTime == 0)
            {
                return DEFAULT_SUBNAUTICA_TIME;
            }
            if (freezeTime)
            {
                return latestRegisteredTime;
            }
            return (DateTimeOffset.UtcNow - latestRegistrationTime).TotalMilliseconds * 0.001 + latestRegisteredTime;
        }
    }

    /// <summary>
    ///     Real deltaTime between two updates of local time through <see cref="DayNightCycle_Update_Patch"/>
    /// </summary>
    /// <remarks>
    /// <para>
    ///     Replaces <see cref="Time.deltaTime"/> because it is capped by <see cref="Time.maximumDeltaTime"/>
    ///     and may not reflect the real time which has passed between two frames once it's higher than the said maximum
    ///     <br/>See <a href="https://docs.unity3d.com/ScriptReference/Time-maximumDeltaTime.html">Time.maximumDeltaTime</a>
    /// </para>
    /// <para>
    ///     This value is set to <c>0</c> when a time skip occurs to avoid undesired behaviours
    ///     (e.g. consuming a day worth of energy just when you skipped 24 in-game hours)
    /// </para>
    /// </remarks>
    public float DeltaTime = 0;

    public void ProcessUpdate(TimeChange packet)
    {
        if (freezeTime && Multiplayer.Main && Multiplayer.Main.InitialSyncCompleted)
        {
            freezeTime = false;
        }
        realTimeElapsedRegistrationTime = DateTimeOffset.FromUnixTimeMilliseconds(packet.UpdateTime);
        realTimeElapsed = packet.RealTimeElapsed;

        latestRegistrationTime = DateTimeOffset.FromUnixTimeMilliseconds(packet.UpdateTime);
        latestRegisteredTime = packet.CurrentTime;
        
        DayNightCycle.main.timePassedAsDouble = CalculateCurrentTime();
        // We don't want to have a big DeltaTime when processing a time skip
        DeltaTime = 0;

        DayNightCycle.main.StopSkipTimeMode();
    }

    /// <remarks>
    /// Sets <see cref="DeltaTime"/> accordingly
    /// </remarks>
    /// <returns>The newly calculated time from <see cref="CurrentTime"/></returns>
    public double CalculateCurrentTime()
    {
        double currentTime = CurrentTime;
        DeltaTime = (float)(currentTime - DayNightCycle.main.timePassedAsDouble);
        return currentTime;
    }

    public void InitRealTimeElapsed(double realTimeElapsed, long registrationTime, bool isFirstPlayer)
    {
        this.realTimeElapsed = realTimeElapsed;
        realTimeElapsedRegistrationTime = DateTimeOffset.FromUnixTimeMilliseconds(registrationTime);
        freezeTime = isFirstPlayer;
    }
}

using System;

namespace NitroxModel.Packets;

[Serializable]
public class TimeChange : Packet
{
    /// <summary>
    /// Time in seconds
    /// </summary>
    public double CurrentTime { get; }
    /// <summary>
    /// Real time at which the CurrentTime was observed
    /// </summary>
    public long UpdateTime { get; }
    /// <summary>
    /// Real time elapsed in seconds
    /// </summary>
    public double RealTimeElapsed { get; }

    public bool OnlineMode { get; }
    /// <summary>
    /// UTC offset correction with global NTP servers
    /// </summary>
    public long UtcCorrectionTicks { get; }

    public TimeChange(double currentTime, long updateTime, double realTimeElapsed, bool onlineMode, long utcCorrectionTicks)
    {
        CurrentTime = currentTime;
        UpdateTime = updateTime;
        RealTimeElapsed = realTimeElapsed;
        OnlineMode = onlineMode;
        UtcCorrectionTicks = utcCorrectionTicks;
    }
}

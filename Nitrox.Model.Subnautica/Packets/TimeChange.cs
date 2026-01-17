using System;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class TimeChange : Packet
{
    /// <summary>
    ///     Game time which is the <see cref="RealTimeElapsed" /> with the default game start offset.
    /// </summary>
    public double CurrentTime { get; }

    /// <summary>
    ///     Real time at which the CurrentTime was observed
    /// </summary>
    public long UpdateTime { get; }

    /// <summary>
    ///     Total active time in seconds that the server has been simulating the game.
    /// </summary>
    public double RealTimeElapsed { get; }

    public bool OnlineMode { get; }

    /// <summary>
    ///     UTC offset correction with global NTP servers
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

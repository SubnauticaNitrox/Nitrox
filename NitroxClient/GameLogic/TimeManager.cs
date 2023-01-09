using NitroxModel.Packets;
using System;

namespace NitroxClient.GameLogic;

public class TimeManager
{
    /// <summary>
    /// Latest moment at which we updated the time
    /// </summary>
    public DateTimeOffset LatestRegistrationTime { get; private set; }
    /// <summary>
    /// Latest registered value of the time
    /// </summary>
    private double latestRegisteredTime;

    public double CurrentTime => (DateTimeOffset.Now - LatestRegistrationTime).TotalMilliseconds * 0.001 + latestRegisteredTime;

    public void ProcessUpdate(TimeChange packet)
    {
        LatestRegistrationTime = DateTimeOffset.FromUnixTimeMilliseconds(packet.UpdateTime);
        latestRegisteredTime = packet.CurrentTime;

        DayNightCycle.main.StopSkipTimeMode();
    }
}

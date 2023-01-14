using NitroxModel.Packets;
using System;

namespace NitroxClient.GameLogic;

public class TimeManager
{
    /// <summary>
    /// Latest moment at which we updated the time
    /// </summary>
    private DateTimeOffset latestRegistrationTime;
    /// <summary>
    /// Latest registered value of the time
    /// </summary>
    private double latestRegisteredTime;

    public double CurrentTime
    {
        get
        {
            // Unitialized state
            if (latestRegisteredTime == 0)
            {
                return 480;
            }
            return (DateTimeOffset.Now - latestRegistrationTime).TotalMilliseconds * 0.001 + latestRegisteredTime;
        }
    }

    public void ProcessUpdate(TimeChange packet)
    {
        latestRegistrationTime = DateTimeOffset.FromUnixTimeMilliseconds(packet.UpdateTime);
        latestRegisteredTime = packet.CurrentTime;
        
        DayNightCycle.main.StopSkipTimeMode();
    }
}

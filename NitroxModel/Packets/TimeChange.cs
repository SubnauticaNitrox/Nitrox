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

    public TimeChange(double currentTime, long updateTime)
    {
        CurrentTime = currentTime;
        UpdateTime = updateTime;
    }
}

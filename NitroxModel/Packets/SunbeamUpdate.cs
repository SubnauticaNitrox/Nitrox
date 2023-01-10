using System;

namespace NitroxModel.Packets;

[Serializable]
public class SunbeamUpdate : Packet
{
    /// <summary>
    /// Whether the actual time if countdown is active, or -1
    /// </summary>
    public float CountdownStartingTime { get; set; }

    public SunbeamUpdate(float countdownStartingTime)
    {
        CountdownStartingTime = countdownStartingTime;
    }
}

using System;
using NitroxModel.Packets;

namespace NitroxModel.DataStructures.GameLogic;

[Serializable]
public class TimeData
{
    public TimeChange TimePacket;
    public AuroraEventData AuroraEventData;
    public double RealTimeElapsed;

    public TimeData(TimeChange timePacket, AuroraEventData auroraEventData, double realTimeElapsed)
    {
        TimePacket = timePacket;
        AuroraEventData = auroraEventData;
        RealTimeElapsed = realTimeElapsed;
    }
}

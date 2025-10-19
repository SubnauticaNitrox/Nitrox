using System;
using Nitrox.Model.Packets;

namespace Nitrox.Model.DataStructures.GameLogic;

[Serializable]
public class TimeData
{
    public TimeChange TimePacket;
    public AuroraEventData AuroraEventData;

    public TimeData(TimeChange timePacket, AuroraEventData auroraEventData)
    {
        TimePacket = timePacket;
        AuroraEventData = auroraEventData;
    }
}

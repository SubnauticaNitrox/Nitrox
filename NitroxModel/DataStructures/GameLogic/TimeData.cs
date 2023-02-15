using System;
using NitroxModel.Packets;

namespace NitroxModel.DataStructures.GameLogic;

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

using System;
using NitroxModel.Packets;

namespace NitroxModel.DataStructures.GameLogic;

[Serializable]
public class TimeData
{
    public TimeChange TimePacket;
#if SUBNAUTICA
    public AuroraEventData AuroraEventData;

    public TimeData(TimeChange timePacket, AuroraEventData auroraEventData)
    {
        TimePacket = timePacket;
        AuroraEventData = auroraEventData;
    }
#elif BELOWZERO
    public TimeData(TimeChange timePacket)
    {
        TimePacket = timePacket;
    }
#endif
}

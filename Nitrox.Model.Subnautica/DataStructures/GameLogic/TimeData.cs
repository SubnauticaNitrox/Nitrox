using System;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.Packets;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic;

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

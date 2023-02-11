using System;
using NitroxModel.Packets;

namespace NitroxModel.DataStructures.GameLogic;

[Serializable]
public class TimeData
{
    public TimeChange TimePacket;
    public AuroraEventData CrashedShipExploderData;

    public TimeData(TimeChange timePacket, AuroraEventData crashedShipExploderData)
    {
        TimePacket = timePacket;
        CrashedShipExploderData = crashedShipExploderData;
    }
}

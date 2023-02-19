using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets;

[Serializable]
public class AuroraAndTimeUpdate : Packet
{
    public TimeData TimeData { get; }
    public bool Restore { get; }

    public AuroraAndTimeUpdate(TimeData timeData, bool restore)
    {
        TimeData = timeData;
        Restore = restore;
    }
}

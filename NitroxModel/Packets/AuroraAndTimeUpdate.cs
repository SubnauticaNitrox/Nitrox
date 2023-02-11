using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets;

[Serializable]
public class AuroraAndTimeUpdate : Packet
{
    public TimeData InitialTimeData { get; }
    public bool Restore { get; }

    public AuroraAndTimeUpdate(TimeData initialTimeData, bool restore)
    {
        InitialTimeData = initialTimeData;
        Restore = restore;
    }
}

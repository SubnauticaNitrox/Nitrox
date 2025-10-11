using System;
using Nitrox.Model.DataStructures.GameLogic;

namespace Nitrox.Model.Packets;

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

using System;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Model.Subnautica.Packets;

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

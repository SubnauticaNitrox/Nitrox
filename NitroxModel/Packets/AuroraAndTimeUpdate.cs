using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets;

[Serializable]
public class AuroraAndTimeUpdate : Packet
{
    public InitialTimeData InitialTimeData { get; }
    public bool Restore { get; }

    public AuroraAndTimeUpdate(InitialTimeData initialTimeData, bool restore)
    {
        InitialTimeData = initialTimeData;
        Restore = restore;
    }
}

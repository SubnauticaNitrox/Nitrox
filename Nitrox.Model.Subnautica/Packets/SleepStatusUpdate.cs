using System;
using BinaryPack.Attributes;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class SleepStatusUpdate : Packet
{
    public int SleepingPlayers { get; }
    public int TotalPlayers { get; }

    [IgnoredMember]
    public bool AllPlayersSleeping => SleepingPlayers >= TotalPlayers;

    public bool WasCancelled { get; }

    public SleepStatusUpdate(int sleepingPlayers, int totalPlayers, bool wasCancelled = false)
    {
        SleepingPlayers = sleepingPlayers;
        TotalPlayers = totalPlayers;
        WasCancelled = wasCancelled;
    }
}

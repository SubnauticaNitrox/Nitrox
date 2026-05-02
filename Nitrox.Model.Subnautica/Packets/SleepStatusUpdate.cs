using System;
using BinaryPack.Attributes;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class SleepStatusUpdate : Packet
{
    public int PlayersInBed { get; }
    public int TotalPlayers { get; }

    [IgnoredMember]
    public bool AllPlayersInBed => PlayersInBed >= TotalPlayers;

    public SleepStatusUpdate(int playersInBed, int totalPlayers)
    {
        PlayersInBed = playersInBed;
        TotalPlayers = totalPlayers;
    }
}

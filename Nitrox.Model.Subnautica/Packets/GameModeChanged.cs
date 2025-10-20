using System;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Packets;
using Nitrox.Model.Server;
using Nitrox.Model.Subnautica.DataStructures;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class GameModeChanged : Packet
{
    public ushort PlayerId { get; }
    public bool AllPlayers { get; }
    public SubnauticaGameMode GameMode { get; }

    public GameModeChanged(ushort playerId, bool allPlayers, SubnauticaGameMode gameMode)
    {
        PlayerId = playerId;
        AllPlayers = allPlayers;
        GameMode = gameMode;
    }

    public static GameModeChanged ForPlayer(ushort playerId, SubnauticaGameMode gameMode)
    {
        return new(playerId, false, gameMode);
    }

    public static GameModeChanged ForAllPlayers(SubnauticaGameMode gameMode)
    {
        return new(0, true, gameMode);
    }
}

using System;
using Nitrox.Model.Packets;
using Nitrox.Model.Server;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class GameModeChanged : Packet
{
    public ushort PlayerId { get; }
    public bool AllPlayers { get; }
    public NitroxGameMode GameMode { get; }

    public GameModeChanged(ushort playerId, bool allPlayers, NitroxGameMode gameMode)
    {
        PlayerId = playerId;
        AllPlayers = allPlayers;
        GameMode = gameMode;
    }

    public static GameModeChanged ForPlayer(ushort playerId, NitroxGameMode gameMode)
    {
        return new(playerId, false, gameMode);
    }

    public static GameModeChanged ForAllPlayers(NitroxGameMode gameMode)
    {
        return new(0, true, gameMode);
    }
}

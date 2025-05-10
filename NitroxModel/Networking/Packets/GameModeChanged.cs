using System;
using NitroxModel.Server;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record GameModeChanged : Packet
{
    public SessionId PlayerId { get; }
    public bool AllPlayers { get; }
    public SubnauticaGameMode GameMode { get; }

    public GameModeChanged(SessionId playerId, bool allPlayers, SubnauticaGameMode gameMode)
    {
        PlayerId = playerId;
        AllPlayers = allPlayers;
        GameMode = gameMode;
    }

    public static GameModeChanged ForPlayer(SessionId playerId, SubnauticaGameMode gameMode)
    {
        return new(playerId, false, gameMode);
    }

    public static GameModeChanged ForAllPlayers(SubnauticaGameMode gameMode)
    {
        return new(0, true, gameMode);
    }
}

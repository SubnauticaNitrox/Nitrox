using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public sealed class GameModeChanged : Packet
{
    public SessionId SessionId { get; }
    public bool AllPlayers { get; }
    public SubnauticaGameMode GameMode { get; }

    public GameModeChanged(SessionId sessionId, bool allPlayers, SubnauticaGameMode gameMode)
    {
        SessionId = sessionId;
        AllPlayers = allPlayers;
        GameMode = gameMode;
    }

    public static GameModeChanged ForPlayer(SessionId sessionId, SubnauticaGameMode gameMode)
    {
        return new(sessionId, false, gameMode);
    }

    public static GameModeChanged ForAllPlayers(SubnauticaGameMode gameMode)
    {
        return new(0, true, gameMode);
    }
}

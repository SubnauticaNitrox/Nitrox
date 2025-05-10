using Nitrox.Server.Subnautica.Database.Models;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Dto;
using NitroxModel.Server;

namespace Nitrox.Server.Subnautica.Models.Respositories.Core;

internal interface IPlayerRepository
{
    Task<PlayContext> GetPlayContextById(SessionId sessionId);
    Task<ConnectedPlayerDto[]> GetConnectedPlayersAsync();

    /// <summary>
    ///     Gets the players with the given name. Since multiple players can share the same name, returns an array.
    /// </summary>
    Task<ConnectedPlayerDto[]> GetConnectedPlayersByNameAsync(string name);

    Task<ConnectedPlayerDto> GetConnectedPlayerByPlayerIdAsync(PeerId playerId);
    Task<ConnectedPlayerDto> GetConnectedPlayerBySessionIdAsync(SessionId sessionId);
    Task<string> GetPlayerNameIfNotMuted(PeerId playerId);
    Task<bool> SetPlayerPermissions(PeerId playerId, Perms permissions);
    Task<bool> SetPlayerGameMode(PeerId playerId, SubnauticaGameMode gameMode);
    Task<bool> SetPlayerMuted(PeerId playerId, bool mute);
}

using Nitrox.Model.Core;
using Nitrox.Server.Subnautica.Models.Commands.ArgConverters.Core;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands.ArgConverters;

/// <summary>
///     Converts a player name to a player session id, if known.
/// </summary>
internal sealed class PlayerNameToSessionIdArgConverter(PlayerService playerService) : IArgConverter<string, SessionId>
{
    private readonly PlayerService playerService = playerService;

    public Task<ConvertResult> ConvertAsync(string playerName)
    {
        if (!playerService.TryGetPlayerByName(playerName, out SessionId? player))
        {
            return Task.FromResult(ConvertResult.Fail($"No player found by name '{playerName}'"));
        }
        return Task.FromResult(ConvertResult.Ok(player));
    }
}

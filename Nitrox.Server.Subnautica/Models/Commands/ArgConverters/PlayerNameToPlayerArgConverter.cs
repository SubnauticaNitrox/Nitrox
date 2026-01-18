using Nitrox.Server.Subnautica.Models.Commands.ArgConverters.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands.ArgConverters;

/// <summary>
///     Converts a player name to a player object, if known.
/// </summary>
internal class PlayerNameToPlayerArgConverter(PlayerManager playerManager) : IArgConverter<string, Player>
{
    private readonly PlayerManager playerManager = playerManager;

    public async Task<ConvertResult> ConvertAsync(string playerName)
    {
        if (!playerManager.TryGetPlayerByName(playerName, out Player? player))
        {
            return ConvertResult.Fail($"No player found by name '{playerName}'");
        }
        return ConvertResult.Ok(player);
    }
}

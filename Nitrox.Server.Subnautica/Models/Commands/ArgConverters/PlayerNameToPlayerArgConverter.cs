using Nitrox.Server.Subnautica.Models.Commands.ArgConverters.Core;
using Nitrox.Server.Subnautica.Models.Respositories;
using NitroxModel.Dto;

namespace Nitrox.Server.Subnautica.Models.Commands.ArgConverters;

/// <summary>
///     Converts a player name to a player object, if known.
/// </summary>
internal class PlayerNameToPlayerArgConverter(PlayerRepository playerRepository) : IArgConverter<string, ConnectedPlayerDto>
{
    private readonly PlayerRepository playerRepository = playerRepository;

    public async Task<ConvertResult> ConvertAsync(string playerId)
    {
        ConnectedPlayerDto[] player = await playerRepository.GetConnectedPlayersByNameAsync(playerId);
        if (player == null)
        {
            return ConvertResult.Fail($"No player found by name '{playerId}'");
        }
        return ConvertResult.Ok(player);
    }
}

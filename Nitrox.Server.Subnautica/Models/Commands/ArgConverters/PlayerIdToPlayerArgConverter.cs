using Nitrox.Server.Subnautica.Models.Commands.ArgConverters.Core;
using Nitrox.Server.Subnautica.Models.Respositories;
using NitroxModel.Dto;

namespace Nitrox.Server.Subnautica.Models.Commands.ArgConverters;

/// <summary>
///     Converts a player ID to a player object, if known.
/// </summary>
internal class PlayerIdToPlayerArgConverter(PlayerRepository playerRepository) : IArgConverter<ushort, ConnectedPlayerDto>
{
    private readonly PlayerRepository playerRepository = playerRepository;

    public async Task<ConvertResult> ConvertAsync(ushort playerId)
    {
        ConnectedPlayerDto player = await playerRepository.GetConnectedPlayerBySessionIdAsync(playerId);
        if (player == null)
        {
            return ConvertResult.Fail($"No player found with ID {playerId}");
        }

        return ConvertResult.Ok(player);
    }
}

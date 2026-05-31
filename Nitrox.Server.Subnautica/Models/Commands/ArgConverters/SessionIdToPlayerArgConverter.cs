using Nitrox.Server.Subnautica.Models.Commands.ArgConverters.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands.ArgConverters;

/// <summary>
///     Converts a player ID to a player object, if known.
/// </summary>
internal sealed class SessionIdToPlayerArgConverter(PlayerManager playerManager) : IArgConverter<ushort, Player>
{
    private readonly PlayerManager playerManager = playerManager;

    public Task<ConvertResult> ConvertAsync(ushort sessionId)
    {
        if (sessionId < 1)
        {
            return Task.FromResult(ConvertResult.Fail("Session id must start with 1"));
        }
        if (!playerManager.TryGetPlayerBySessionId(sessionId, out Player player))
        {
            return Task.FromResult(ConvertResult.Fail($"No player found by session #{sessionId}"));
        }

        return Task.FromResult(ConvertResult.Ok(player));
    }
}

using Nitrox.Model.Core;
using Nitrox.Server.Subnautica.Models.Commands.ArgConverters.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands.ArgConverters;

/// <summary>
///     Converts a player ID to a player object, if known.
/// </summary>
internal class SessionIdToPlayerArgConverter(PlayerManager playerManager) : IArgConverter<SessionId, Player>
{
    private readonly PlayerManager playerManager = playerManager;

    public Task<ConvertResult> ConvertAsync(SessionId sessionId)
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

using Nitrox.Model.Core;
using Nitrox.Server.Subnautica.Models.Commands.ArgConverters.Core;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands.ArgConverters;

/// <summary>
///     Converts a string to a NitroxId, if valid.
/// </summary>
internal sealed class StringToSessionIdConverter(PlayerService playerService) : IArgConverter<string, SessionId>
{
    private readonly PlayerService playerService = playerService;

    public Task<ConvertResult> ConvertAsync(string sessionIdStr)
    {
        if (!ushort.TryParse(sessionIdStr, out ushort sessionIdNum))
        {
            return Task.FromResult(ConvertResult.Fail($"Can not convert to {nameof(SessionId)}"));
        }
        SessionId sessionId = (SessionId)sessionIdNum;
        if (playerService.GetPlayerReservation(sessionId) == null)
        {
            return Task.FromResult(ConvertResult.Fail($"No player connected on session #{sessionId}"));
        }
        return Task.FromResult(ConvertResult.Ok(sessionId));
    }
}

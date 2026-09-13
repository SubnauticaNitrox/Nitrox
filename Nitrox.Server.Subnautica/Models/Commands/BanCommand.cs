using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Administration;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Communication;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
internal sealed class BanCommand(PlayerService playerService, SessionManager sessionManager, IBan ban, IKickPlayer playerKicker)
    : ICommandHandler<SessionId, string, TimeSpan>, ICommandHandler<IPAddress, string, TimeSpan>
{
    private readonly IBan ban = ban;

    [Description("Bans an online player by their current IP address, kicking them")]
    public async Task Execute(ICommandContext context,
                              [Description("Player to ban")] SessionId target,
                              [Description("Ban reason")] string reason = "",
                              [Description("Duration like 30m/12h/7d/2w, omit for permanent")]
                              TimeSpan duration = default)
    {
        string playerName = playerService.GetProperty<NameProperty>(target).Value;
        IPEndPoint? endPoint = sessionManager.GetEndPoint(target);
        if (endPoint is null)
        {
            await context.ReplyAsync($"Could not determine the IP address of '{playerName}'");
            return;
        }

        await BanAddressAsync(context, endPoint.Address, duration, reason, playerName);
    }

    [Description("Bans a raw IP address, kicking anyone currently connected from it")]
    public async Task Execute(ICommandContext context,
                              [Description("IP address to ban")] IPAddress target,
                              [Description("Ban reason")] string reason = "",
                              [Description("Duration like 30m/12h/7d/2w, omit for permanent")]
                              TimeSpan duration = default) =>
        await BanAddressAsync(context, target, duration, reason);

    /// <summary>
    ///     Bans an IP address. Every player currently connected from that IP is kicked; the ban itself is purely
    ///     IP-based so reconnecting under a different name stays blocked.
    /// </summary>
    private async Task BanAddressAsync(ICommandContext context, IPAddress ip, TimeSpan duration, string? reason, string? playerName = null)
    {
        List<SessionId> sessionsOnTargetIp = [];
        foreach (SessionId sessionId in playerService.GetSessionIds())
        {
            if (ip.Equals(sessionManager.GetEndPoint(sessionId)?.Address))
            {
                sessionsOnTargetIp.Add(sessionId);
            }
        }
        foreach (SessionId sessionId in sessionsOnTargetIp)
        {
            if (context.OriginId == sessionId)
            {
                await context.ReplyAsync("You can't ban yourself");
                return;
            }
        }

        SessionId? outranking = null;
        foreach (SessionId sessionId in sessionsOnTargetIp)
        {
            if (context.Permissions <= playerService.GetProperty<PermissionsProperty>(sessionId).Value)
            {
                outranking = sessionId;
                break;
            }
        }
        if (outranking.HasValue)
        {
            await context.ReplyAsync($"You're not allowed to ban {playerService.GetProperty<NameProperty>(outranking.Value).Value} #{outranking.Value}");
            return;
        }

        if (NitroxEnvironment.IsReleaseMode && ip.IsPrivate())
        {
            SessionId? player = null;
            foreach (SessionId id in sessionsOnTargetIp)
            {
                player = id;
                break;
            }
            if (player.HasValue)
            {
                await context.ReplyAsync($"Player '{playerService.GetProperty<NameProperty>(player.Value).Value}' connected with a private IP address and can't be banned");
            }
            else
            {
                await context.ReplyAsync("IP address is private and can't be banned");
            }
            return;
        }

        playerName ??= sessionsOnTargetIp.Count == 1 ? playerService.GetProperty<NameProperty>(sessionsOnTargetIp[0]).Value : null;
        reason = reason?.Trim();
        await ban.BanAsync(ip, duration, context.OriginName, reason, playerName);
        foreach (SessionId player in sessionsOnTargetIp)
        {
            await playerKicker.KickPlayer(player, string.IsNullOrEmpty(reason) ? "Banned" : $"Banned: {reason}");
        }

        string durationText = duration != TimeSpan.Zero ? $"for {duration}" : "permanently";
        string reasonText = string.IsNullOrEmpty(reason) ? "" : $" - {reason}";
        string targetText = string.IsNullOrEmpty(playerName) ? ip.ToString() : $"{playerName} ({ip})";
        await context.ReplyAsync($"Banned {targetText} {durationText}{reasonText}");
    }
}

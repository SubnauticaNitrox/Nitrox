using System.ComponentModel;
using System.Linq;
using System.Net;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Administration;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Communication;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
internal sealed class BanCommand(PlayerManager playerManager, SessionManager sessionManager, BanService banService, IKickPlayer playerKicker)
    : ICommandHandler<Player, string, TimeSpan>, ICommandHandler<IPAddress, string, TimeSpan>
{
    [Description("Bans an online player by their current IP address, kicking them")]
    public async Task Execute(ICommandContext context,
                              [Description("Player to ban")] Player target,
                              [Description("Ban reason")] string reason = "",
                              [Description("Duration like 30m/12h/7d/2w, omit for permanent")]
                              TimeSpan duration = default)
    {
        IPEndPoint endPoint = sessionManager.GetEndPoint(target.SessionId);
        if (endPoint is null)
        {
            await context.ReplyAsync($"Could not determine the IP address of '{target.Name}'");
            return;
        }

        await BanAddressAsync(context, endPoint.Address, target.Name, reason, duration);
    }

    [Description("Bans a raw IP address, kicking anyone currently connected from it")]
    public async Task Execute(ICommandContext context,
                              [Description("IP address to ban")] IPAddress target,
                              [Description("Ban reason")] string reason = "",
                              [Description("Duration like 30m/12h/7d/2w, omit for permanent")]
                              TimeSpan duration = default) =>
        await BanAddressAsync(context, target, null, reason, duration);

    /// <summary>
    ///     Bans an IP address. Every player currently connected from that IP is kicked; the ban itself is purely
    ///     IP-based so reconnecting under a different name stays blocked.
    /// </summary>
    private async Task BanAddressAsync(ICommandContext context, IPAddress ip, string? label, string? reason, TimeSpan duration)
    {
        Player[] connectedFromIp = playerManager.GetConnectedPlayers()
                                                .Where(player => ip.Equals(sessionManager.GetEndPoint(player.SessionId)?.Address))
                                                .ToArray();
        if (connectedFromIp.Any(player => context.OriginId == player.SessionId))
        {
            await context.ReplyAsync("You can't ban yourself");
            return;
        }
        Player outranking = connectedFromIp.FirstOrDefault(player => context.Permissions <= player.Permissions);
        if (outranking != null)
        {
            await context.ReplyAsync($"You're not allowed to ban {outranking.Name}");
            return;
        }
        if (NitroxEnvironment.IsReleaseMode && ip.IsPrivate())
        {
            Player? player = connectedFromIp.FirstOrDefault();
            if (player != null)
            {
                await context.ReplyAsync($"Player '{player.Name}' connected with a private IP address and can't be banned");
            }
            else
            {
                await context.ReplyAsync("IP address is private and can't be banned");
            }
            return;
        }

        label ??= connectedFromIp.Length == 1 ? connectedFromIp[0].Name : null;
        reason = reason?.Trim();
        await banService.BanAsync(ip, label, reason, context.OriginName, duration);
        foreach (Player player in connectedFromIp.Where(player => player.IsOnline))
        {
            await playerKicker.KickPlayer(player.SessionId, string.IsNullOrEmpty(reason) ? "Banned" : $"Banned: {reason}");
        }

        string durationText = duration != TimeSpan.Zero ? $"for {duration}" : "permanently";
        string reasonText = string.IsNullOrEmpty(reason) ? "" : $" - {reason}";
        string targetText = string.IsNullOrEmpty(label) ? ip.ToString() : $"{label} ({ip})";
        await context.ReplyAsync($"Banned {targetText} {durationText}{reasonText}");
    }
}

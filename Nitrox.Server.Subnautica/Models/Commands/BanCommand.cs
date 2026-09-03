using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Administration;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Communication;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
internal sealed partial class BanCommand(PlayerManager playerManager, SessionManager sessionManager, BanService banService, IKickPlayer playerKicker)
    : ICommandHandler<Player, string, string>, ICommandHandler<IPAddress, string, string>
{
    [GeneratedRegex(@"^(\d+)(s|m|h|d|w)$", RegexOptions.IgnoreCase)]
    private static partial Regex DurationRegex();

    [Description("Bans an online player by their current IP address, kicking them")]
    public async Task Execute(ICommandContext context,
                              [Description("Player to ban")] Player target,
                              [Description("Ban reason")] string reason = "",
                              [Description("Duration like 30m/12h/7d/2w, omit for permanent")] string duration = "")
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
                              [Description("Duration like 30m/12h/7d/2w, omit for permanent")] string duration = "")
    {
        await BanAddressAsync(context, target, null, reason, duration);
    }

    /// <summary>
    ///     Bans an IP address. Every player currently connected from that IP is kicked; the ban itself is purely
    ///     IP-based so reconnecting under a different name stays blocked.
    /// </summary>
    private async Task BanAddressAsync(ICommandContext context, IPAddress ip, string label, string reason, string duration)
    {
        if (!TryParseDuration(duration, out TimeSpan? parsedDuration))
        {
            await context.ReplyAsync($"Invalid duration '{duration}'. Use a number followed by s/m/h/d/w (e.g. 7d), or omit for a permanent ban");
            return;
        }

        List<Player> connectedFromIp = playerManager.GetConnectedPlayers()
                                                    .Where(player => ip.Equals(sessionManager.GetEndPoint(player.SessionId)?.Address))
                                                    .ToList();

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

        label ??= connectedFromIp.Count == 1 ? connectedFromIp[0].Name : null;

        await banService.BanAsync(ip, label, reason, context.OriginName, parsedDuration);

        foreach (Player player in connectedFromIp.Where(player => player.IsOnline))
        {
            await playerKicker.KickPlayer(player.SessionId, string.IsNullOrEmpty(reason) ? "Banned" : $"Banned: {reason}");
        }

        string durationText = parsedDuration.HasValue ? $"for {duration}" : "permanently";
        string reasonText = string.IsNullOrEmpty(reason) ? "" : $" - {reason}";
        string targetText = string.IsNullOrEmpty(label) ? ip.ToString() : $"{label} ({ip})";
        await context.ReplyAsync($"Banned {targetText} {durationText}{reasonText}");
    }

    private static bool TryParseDuration(string duration, out TimeSpan? parsed)
    {
        if (string.IsNullOrWhiteSpace(duration))
        {
            parsed = null;
            return true;
        }

        Match match = DurationRegex().Match(duration);
        if (!match.Success)
        {
            parsed = null;
            return false;
        }

        int amount = int.Parse(match.Groups[1].Value);
        parsed = match.Groups[2].Value.ToLowerInvariant() switch
        {
            "s" => TimeSpan.FromSeconds(amount),
            "m" => TimeSpan.FromMinutes(amount),
            "h" => TimeSpan.FromHours(amount),
            "d" => TimeSpan.FromDays(amount),
            "w" => TimeSpan.FromDays(amount * 7),
            _ => null
        };
        return true;
    }
}

using System.ComponentModel;
using System.Net;
using System.Text.RegularExpressions;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Administration;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Players.Bans;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
internal sealed partial class BanCommand(PlayerManager playerManager, BanManager banManager, IKickPlayer playerKicker) : ICommandHandler<string, string, string>
{
    [GeneratedRegex(@"^(\d+)(s|m|h|d|w)$", RegexOptions.IgnoreCase)]
    private static partial Regex DurationRegex();

    [Description("Bans a player (online or offline) or a raw IP address, kicking them if currently online")]
    public async Task Execute(ICommandContext context,
                              [Description("Player name or IP address")] string target,
                              [Description("Ban reason")] string reason = "",
                              [Description("Duration like 30m/12h/7d/2w, omit for permanent")] string duration = "")
    {
        if (!TryParseDuration(duration, out TimeSpan? parsedDuration))
        {
            await context.ReplyAsync($"Invalid duration '{duration}'. Use a number followed by s/m/h/d/w (e.g. 7d), or omit for a permanent ban");
            return;
        }

        if (!TryResolveTarget(target, out IPAddress ip, out string playerName, out Player knownPlayer))
        {
            await context.ReplyAsync($"No player or valid IP address found for '{target}'");
            return;
        }

        if (knownPlayer != null)
        {
            if (context.OriginId == knownPlayer.SessionId)
            {
                await context.ReplyAsync("You can't ban yourself");
                return;
            }
            if (context.Permissions <= knownPlayer.Permissions)
            {
                await context.ReplyAsync($"You're not allowed to ban {knownPlayer.Name}");
                return;
            }
        }

        banManager.Ban(ip, playerName, reason, context.OriginName, parsedDuration);

        if (knownPlayer is { IsOnline: true })
        {
            await playerKicker.KickPlayer(knownPlayer.SessionId, string.IsNullOrEmpty(reason) ? "Banned" : $"Banned: {reason}");
        }

        string durationText = parsedDuration.HasValue ? $"for {duration}" : "permanently";
        string reasonText = string.IsNullOrEmpty(reason) ? "" : $" - {reason}";
        await context.ReplyAsync($"Banned '{playerName ?? target}' ({ip}) {durationText}{reasonText}");
    }

    private bool TryResolveTarget(string target, out IPAddress ip, out string playerName, out Player knownPlayer)
    {
        if (playerManager.TryGetPlayerByName(target, out Player onlinePlayer) && IPAddress.TryParse(onlinePlayer.LastKnownIp, out IPAddress onlineIp))
        {
            ip = onlineIp;
            playerName = onlinePlayer.Name;
            knownPlayer = onlinePlayer;
            return true;
        }

        if (IPAddress.TryParse(target, out ip))
        {
            playerName = null;
            knownPlayer = null;
            return true;
        }

        foreach (Player player in playerManager.GetAllPlayers())
        {
            if (!string.Equals(player.Name, target, StringComparison.OrdinalIgnoreCase) || !IPAddress.TryParse(player.LastKnownIp, out IPAddress offlineIp))
            {
                continue;
            }
            ip = offlineIp;
            playerName = player.Name;
            knownPlayer = player;
            return true;
        }

        ip = null;
        playerName = null;
        knownPlayer = null;
        return false;
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

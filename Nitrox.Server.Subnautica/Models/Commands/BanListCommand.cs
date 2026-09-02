using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.GameLogic.Players.Bans;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
internal sealed class BanListCommand(BanManager banManager) : ICommandHandler
{
    [Description("Lists all active bans")]
    public async Task Execute(ICommandContext context)
    {
        IReadOnlyList<BanEntry> bans = banManager.GetActiveBans();
        if (bans.Count == 0)
        {
            await context.ReplyAsync("No active bans");
            return;
        }

        StringBuilder builder = new($"Active bans ({bans.Count}):\n");
        foreach (BanEntry ban in bans)
        {
            string playerName = string.IsNullOrEmpty(ban.PlayerName) ? "?" : ban.PlayerName;
            string expiry = ban.ExpiresAtUtc.HasValue ? $"expires {ban.ExpiresAtUtc.Value:u}" : "permanent";
            string reason = string.IsNullOrEmpty(ban.Reason) ? "" : $" - {ban.Reason}";
            builder.AppendLine($"{playerName} ({ban.IpAddress}) - {expiry}{reason}");
        }

        await context.ReplyAsync(builder.ToString());
    }
}

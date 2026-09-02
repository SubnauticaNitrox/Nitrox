using System.ComponentModel;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.GameLogic.Players.Bans;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
internal sealed class UnbanCommand(BanManager banManager) : ICommandHandler<string>
{
    [Description("Removes a ban by player name or IP address")]
    public async Task Execute(ICommandContext context, [Description("Player name or IP address")] string target)
    {
        if (!banManager.Unban(target))
        {
            await context.ReplyAsync($"No active ban found for '{target}'");
            return;
        }
        await context.ReplyAsync($"Removed ban for '{target}'");
    }
}

using System.ComponentModel;
using System.Net;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
internal sealed class UnbanCommand(BanService banService) : ICommandHandler<IPAddress>
{
    [Description("Removes the ban on an IP address (see banlist for the address)")]
    public async Task Execute(ICommandContext context, [Description("Banned IP address")] IPAddress target)
    {
        if (!await banService.UnbanAsync(target))
        {
            await context.ReplyAsync($"No active ban found for {target}");
            return;
        }
        await context.ReplyAsync($"Removed ban for {target}");
    }
}

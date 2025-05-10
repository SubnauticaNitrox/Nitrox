using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using NitroxModel.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
internal class SummaryCommand : ICommandHandler
{
    [Description("Shows persisted data")]
    public Task Execute(ICommandContext context)
    {
        // TODO: Fix save summary
        // context.Reply(server.GetSaveSummary(context.Permissions));

        return Task.CompletedTask;
    }
}

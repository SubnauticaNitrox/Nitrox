using System.ComponentModel;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Logging.Scopes;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
internal sealed class SummaryCommand(StatusService statusService, ILogger<SummaryCommand> logger) : ICommandHandler
{
    private readonly StatusService statusService = statusService;
    private readonly ILogger<SummaryCommand> logger = logger;

    [Description("Shows persisted data")]
    public async Task Execute(ICommandContext context)
    {
        string summary;
        using (CaptureScope scope = logger.BeginCaptureScope())
        {
            await statusService.LogServerSummary(context.Permissions);
            summary = string.Join("", scope.Logs);
        }
        await context.ReplyAsync(summary.TrimEnd('\n'));
    }
}

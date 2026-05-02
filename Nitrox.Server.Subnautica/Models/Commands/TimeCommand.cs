using System.ComponentModel;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
internal sealed class TimeCommand(TimeService timeService) : ICommandHandler<StoryManager.TimeModification>
{
    private readonly TimeService timeService = timeService;

    [Description("Changes the map time")]
    public async Task Execute(ICommandContext context, [Description("Changes the map time")] StoryManager.TimeModification time)
    {
        switch (time)
        {
            case StoryManager.TimeModification.DAY:
                timeService.ChangeTime(StoryManager.TimeModification.DAY);
                await context.SendToAllAsync("Time set to day");
                break;
            case StoryManager.TimeModification.NIGHT:
                timeService.ChangeTime(StoryManager.TimeModification.NIGHT);
                await context.SendToAllAsync("Time set to night");
                break;
            default:
                timeService.ChangeTime(StoryManager.TimeModification.SKIP);
                await context.SendToAllAsync("Skipped time");
                break;
        }
    }
}

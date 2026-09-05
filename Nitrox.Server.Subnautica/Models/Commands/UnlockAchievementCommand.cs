using System.ComponentModel;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.PLAYER)]
internal sealed class UnlockAchievementCommand(IOptions<SubnauticaServerOptions> options) : ICommandHandler<string>
{
    [RequiresOrigin(CommandOrigin.PLAYER)]
    [Description("Unlocks an achievement for yourself that you may have missed")]
    public async Task Execute(ICommandContext context, [Description("The achievement's internal name, e.g. DiveForTheVeryFirstTime")] string achievementName)
    {
        if (context is not PlayerToServerCommandContext playerContext)
        {
            return;
        }
        if (options.Value.AchievementsMode == AchievementsMode.NO_ACHIEVEMENTS)
        {
            await context.ReplyAsync("Achievements are disabled on this server.");
            return;
        }
        if (string.IsNullOrWhiteSpace(achievementName))
        {
            await context.ReplyAsync("You must specify an achievement name, e.g. /unlockachievement DiveForTheVeryFirstTime. Run /help unlockachievement for details.");
            return;
        }

        await context.SendAsync(playerContext.Player.SessionId, new UnlockAchievement(achievementName));
    }
}

using System.ComponentModel;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands;

// We shouldn't let the server-side use this command because it needs some stuff to happen client-side (e.g. story goals)
[RequiresPermission(Perms.ADMIN)]
[RequiresOrigin(CommandOrigin.PLAYER)]
internal sealed class AuroraCommand(StoryManager storyManager) : ICommandHandler<AuroraCommand.AuroraAction>
{
    private readonly StoryManager storyManager = storyManager;

    [Description("Which action to apply to Aurora")]
    public async Task Execute(ICommandContext context, AuroraAction action)
    {
        switch (action)
        {
            case AuroraAction.COUNTDOWN:
                await storyManager.BroadcastExplodeAurora(false);
                break;
            case AuroraAction.RESTORE:
                await storyManager.BroadcastRestoreAurora();
                break;
            case AuroraAction.EXPLODE:
                await storyManager.BroadcastExplodeAurora(true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }

    public enum AuroraAction
    {
        COUNTDOWN,
        RESTORE,
        EXPLODE
    }
}

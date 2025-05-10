using System;
using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands;

// We shouldn't let the server-side use this command because it needs some stuff to happen client-side (e.g. story goals)
[RequiresPermission(Perms.ADMIN)]
[RequiresOrigin(CommandOrigin.PLAYER)]
internal class AuroraCommand(StoryTimingService storyManager) : ICommandHandler<AuroraCommand.AuroraAction>
{
    private readonly StoryTimingService storyManager = storyManager;

    [Description("Which action to apply to Aurora")]
    public Task Execute(ICommandContext context, AuroraAction action)
    {
        switch (action)
        {
            case AuroraAction.COUNTDOWN:
                storyManager.BroadcastExplodeAurora(false);
                break;
            case AuroraAction.RESTORE:
                storyManager.BroadcastRestoreAurora();
                break;
            case AuroraAction.EXPLODE:
                storyManager.BroadcastExplodeAurora(true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
        return Task.CompletedTask;
    }

    public enum AuroraAction
    {
        COUNTDOWN,
        RESTORE,
        EXPLODE
    }
}

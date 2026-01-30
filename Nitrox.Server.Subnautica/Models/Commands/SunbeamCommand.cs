using System.ComponentModel;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands;

/// <summary>
///     We shouldn't let the server use this command because it needs some stuff to happen client-side like goals.
/// </summary>
[RequiresPermission(Perms.ADMIN)]
[RequiresOrigin(CommandOrigin.PLAYER)]
internal sealed class SunbeamCommand(StoryManager storyManager) : ICommandHandler<PlaySunbeamEvent.SunbeamEvent>
{
    private readonly StoryManager storyManager = storyManager;

    [Description("Start sunbeam events")]
    public Task Execute(ICommandContext context, [Description("Which Sunbeam event to start")] PlaySunbeamEvent.SunbeamEvent sunbeamEvent)
    {
        storyManager.StartSunbeamEvent(sunbeamEvent.ToStoryKey());

        return Task.CompletedTask;
    }
}

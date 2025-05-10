using System.ComponentModel;
using Nitrox.Model.Subnautica.Extensions;
using Nitrox.Model.Subnautica.Packets;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands;

/// <summary>
///     We shouldn't let the server use this command because it needs some stuff to happen client-side like goals.
/// </summary>
[RequiresPermission(Perms.ADMIN)]
[RequiresOrigin(CommandOrigin.PLAYER)]
internal sealed class SunbeamCommand(StoryTimingService storyTimingService) : ICommandHandler<PlaySunbeamEvent.SunbeamEvent>
{
    private readonly StoryTimingService storyTimingService = storyTimingService;

    [Description("Start sunbeam events")]
    public Task Execute(ICommandContext context, [Description("Which Sunbeam event to start")] PlaySunbeamEvent.SunbeamEvent sunbeamEvent)
    {
        storyTimingService.StartSunbeamEvent(sunbeamEvent.ToSubnauticaStoryKey());

        return Task.CompletedTask;
    }
}

using System;
using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Dto;

namespace Nitrox.Server.Subnautica.Models.Commands;

[Alias("warp")]
[RequiresOrigin(CommandOrigin.PLAYER)]
[RequiresPermission(Perms.MODERATOR)]
public sealed class GoToCommand : ICommandHandler<float, float, float>, ICommandHandler<string>, ICommandHandler<ConnectedPlayerDto>, ICommandHandler<ConnectedPlayerDto, ConnectedPlayerDto>
{
    [Description("Teleports to a player")]
    public async Task Execute(ICommandContext context, ConnectedPlayerDto player)
    {
        // TODO: USE DATABASE
        await context.ReplyAsync($"Received arg {player} of type {player.GetType().Name}");
    }

    [Description("Teleports to a position")]
    public async Task Execute(ICommandContext context, float x, float y, float z)
    {
        // TODO: USE DATABASE
        await context.ReplyAsync($"Teleporting to position {x} {y} {z}...");
    }

    [Description("Teleports to a location given its name")]
    public async Task Execute(ICommandContext context, string subnauticaLocationName)
    {
        // TODO: USE DATABASE
        await context.ReplyAsync($"Teleporting to location {subnauticaLocationName}...");
    }

    [Description("Teleports player A to Player B")]
    [RequiresOrigin(CommandOrigin.ANY)]
    public Task Execute(ICommandContext context, ConnectedPlayerDto playerA, ConnectedPlayerDto playerB)
    {
        // TODO: USE DATABASE
        throw new NotImplementedException();
    }
}

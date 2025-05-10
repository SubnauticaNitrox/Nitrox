using System;
using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Dto;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
internal sealed class KickCommand(LiteNetLibService liteNetLib) : ICommandHandler<ConnectedPlayerDto, string>
{
    [Description("Kicks a player from the server")]
    public async Task Execute(ICommandContext context, ConnectedPlayerDto playerToKick, string reason = "")
    {
        reason ??= "";
        if (context.OriginId == playerToKick.Id)
        {
            await context.ReplyAsync("You can't kick yourself");
            return;
        }

        switch (context.Origin)
        {
            case CommandOrigin.PLAYER when playerToKick.Permissions >= context.Permissions:
                await context.ReplyAsync($"You're not allowed to kick {playerToKick.Name}");
                break;
            case CommandOrigin.PLAYER:
            case CommandOrigin.SERVER:
                if (!await liteNetLib.KickAsync(playerToKick.SessionId, reason))
                {
                    await context.ReplyAsync($"Failed to kick '{playerToKick.Name}'");
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(context.Origin), "Command does not support the issuer origin");
        }
    }
}

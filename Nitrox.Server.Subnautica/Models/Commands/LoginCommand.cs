using System.ComponentModel;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using NitroxModel.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresOrigin(CommandOrigin.PLAYER)]
internal class LoginCommand(IOptions<Configuration.SubnauticaServerOptions> optionsProvider) : ICommandHandler<string>
{
    private readonly IOptions<Configuration.SubnauticaServerOptions> optionsProvider = optionsProvider;

    [Description("Log in to server as admin (requires password)")]
    public async Task Execute(ICommandContext context, [Description("The admin password for the server")] string adminPassword)
    {
        switch (context)
        {
            case PlayerToServerCommandContext { Player: { Permissions: < Perms.ADMIN } player }:
                if (optionsProvider.Value.AdminPassword == adminPassword)
                {
                    // TODO: USE DATABASE
                    // player.Permissions = Perms.ADMIN;
                    await context.ReplyAsync($"You've been made {nameof(Perms.ADMIN)} on this server!");
                }
                else
                {
                    await context.ReplyAsync("Incorrect Password");
                }
                break;
            default:
                await context.ReplyAsync("You already have admin permissions");
                break;
        }
    }
}

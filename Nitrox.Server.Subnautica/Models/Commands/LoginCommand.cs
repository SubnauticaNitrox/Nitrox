using System.ComponentModel;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresOrigin(CommandOrigin.PLAYER)]
internal sealed class LoginCommand(IOptions<SubnauticaServerOptions> optionsProvider) : ICommandHandler<string>
{
    private readonly IOptions<SubnauticaServerOptions> optionsProvider = optionsProvider;

    [Description("Log in to server as admin (requires password)")]
    public async Task Execute(ICommandContext context, [Description("The admin password for the server")] string adminPassword)
    {
        string activePassword = optionsProvider.Value.AdminPassword;
        if (string.IsNullOrWhiteSpace(activePassword))
        {
            await context.ReplyAsync("Logging in with admin password is disabled");
            return;
        }

        switch (context)
        {
            case PlayerToServerCommandContext { Player: { Permissions: < Perms.ADMIN } player }:
                if (activePassword == adminPassword)
                {
                    player.Permissions = Perms.ADMIN;
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

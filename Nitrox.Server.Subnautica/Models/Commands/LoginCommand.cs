using System.ComponentModel;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresOrigin(CommandOrigin.PLAYER)]
internal sealed class LoginCommand(PlayerService playerService, IOptions<SubnauticaServerOptions> optionsProvider) : ICommandHandler<string>
{
    private const Perms LOGIN_GRANTED_PERMS = Perms.ADMIN;
    private readonly PlayerService playerService = playerService;
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
            case PlayerToServerCommandContext { Permissions: < LOGIN_GRANTED_PERMS }:
                if (activePassword == adminPassword)
                {
                    playerService.GetProperty<PermissionsProperty>(context.OriginId).Value = LOGIN_GRANTED_PERMS;
                    await context.ReplyAsync(new PermsChanged(LOGIN_GRANTED_PERMS));
                    await context.ReplyAsync($"You've been made {LOGIN_GRANTED_PERMS} on this server!");
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

using System.ComponentModel;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.ADMIN)]
internal sealed class OpCommand(PlayerService playerService) : ICommandHandler<SessionId>
{
    private readonly PlayerService playerService = playerService;

    public async Task Execute(ICommandContext context, [Description("The players name to make an admin")] SessionId targetPlayer)
    {
        Perms newPerms = Perms.ADMIN;
        playerService.GetProperty<PermissionsProperty>(targetPlayer).Value = newPerms;

        // We need to notify this player that he can show all the admin-related stuff
        await context.SendAsync(targetPlayer, new PermsChanged(newPerms));
        await context.SendAsync(targetPlayer, $"You were promoted to {newPerms}");
        await context.ReplyAsync($"Updated {playerService.GetProperty<NameProperty>(targetPlayer).Value}\'s permissions to {newPerms}");
    }
}

using System.ComponentModel;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.ADMIN)]
internal sealed class OpCommand : ICommandHandler<Player>
{
    public async Task Execute(ICommandContext context, [Description("The players name to make an admin")] Player targetPlayer)
    {
        Perms newPerms = Perms.ADMIN;
        targetPlayer.Permissions = newPerms;

        // We need to notify this player that he can show all the admin-related stuff
        targetPlayer.SendPacket(new PermsChanged(newPerms));
        await context.SendAsync(targetPlayer.SessionId, $"You were promoted to {newPerms}");
        await context.ReplyAsync($"Updated {targetPlayer.Name}\'s permissions to {newPerms}");
    }
}

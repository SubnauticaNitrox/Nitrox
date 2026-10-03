using System.ComponentModel;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands;

internal sealed class PromoteCommand(PlayerService playerService) : ICommandHandler<SessionId, Perms>
{
    private readonly PlayerService playerService = playerService;

    [Description("Sets specific permissions to a user")]
    public async Task Execute(ICommandContext context, [Description("The username to change the permissions of")] SessionId targetPlayer, [Description("Permission level")] Perms newPerms)
    {
        if (context.OriginId == targetPlayer)
        {
            await context.ReplyAsync("You can't promote yourself");
            return;
        }
        if (context.Permissions < newPerms)
        {
            await context.ReplyAsync($"Your permissions ({context.Permissions}) must be higher than the perms you want to assign ({newPerms})");
            return;
        }
        PermissionsProperty targetPermsProperty = playerService.GetProperty<PermissionsProperty>(targetPlayer);
        if (context.Permissions < targetPermsProperty.Value)
        {
            await context.ReplyAsync($"You're not allowed to update {playerService.GetProperty<NameProperty>(targetPlayer).Value}\'s permissions");
            return;
        }

        targetPermsProperty.Value = newPerms;
        await context.SendAsync(targetPlayer, new PermsChanged(newPerms));
        await context.ReplyAsync($"Updated {playerService.GetProperty<NameProperty>(targetPlayer).Value}\'s permissions to {newPerms}");
        await context.SendAsync(targetPlayer, $"You've been promoted to {newPerms}");
    }
}

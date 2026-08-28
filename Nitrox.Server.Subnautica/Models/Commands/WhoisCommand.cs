using System.ComponentModel;
using System.Text;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.PLAYER)]
internal sealed class WhoisCommand : ICommandHandler, ICommandHandler<Player>
{
    [Description("Shows information about a player")]
    public async Task Execute(ICommandContext context, Player targetPlayer)
    {
        StringBuilder builder = new($"==== {targetPlayer.Name} ====\n");
        builder.AppendLine($"Id: {targetPlayer.Entity.Id}");
        builder.AppendLine($"SessionId: {targetPlayer.SessionId}");
        builder.AppendLine($"Role: {targetPlayer.Permissions}");
        builder.AppendLine($"Gamemode: {targetPlayer.GameMode}");
        builder.AppendLine($"Position: {targetPlayer.Position.X}, {targetPlayer.Position.Y}, {targetPlayer.Position.Z}");
        builder.AppendLine($"Oxygen: {targetPlayer.Stats.Oxygen}/{targetPlayer.Stats.MaxOxygen}");
        builder.AppendLine($"Food: {targetPlayer.Stats.Food}");
        builder.AppendLine($"Water: {targetPlayer.Stats.Water}");
        builder.AppendLine($"Infection: {targetPlayer.Stats.InfectionAmount}");
        builder.AppendLine($"In precursor: {targetPlayer.InPrecursor}");

        await context.ReplyAsync(builder.ToString());
    }

    [RequiresOrigin(CommandOrigin.PLAYER)]
    [Description("Shows information about the current player")]
    public async Task Execute(ICommandContext context)
    {
        if (context is not PlayerToServerCommandContext playerContext)
        {
            throw new Exception("Player context is required to run this command");
        }
        await Execute(context, playerContext.Player);
    }
}

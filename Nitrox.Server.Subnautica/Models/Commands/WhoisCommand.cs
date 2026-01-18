using System.ComponentModel;
using System.Text;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.PLAYER)]
internal sealed class WhoisCommand : ICommandHandler<Player>
{
    [Description("Shows informations over a player")]
    public async Task Execute(ICommandContext context, Player targetPlayer)
    {
        StringBuilder builder = new($"==== {targetPlayer.Name} ====\n");
        builder.AppendLine($"ID: {targetPlayer.Id}");
        builder.AppendLine($"Role: {targetPlayer.Permissions}");
        builder.AppendLine($"Position: {targetPlayer.Position.X}, {targetPlayer.Position.Y}, {targetPlayer.Position.Z}");
        builder.AppendLine($"Oxygen: {targetPlayer.Stats.Oxygen}/{targetPlayer.Stats.MaxOxygen}");
        builder.AppendLine($"Food: {targetPlayer.Stats.Food}");
        builder.AppendLine($"Water: {targetPlayer.Stats.Water}");
        builder.AppendLine($"Infection: {targetPlayer.Stats.InfectionAmount}");

        await context.ReplyAsync(builder.ToString());
    }
}

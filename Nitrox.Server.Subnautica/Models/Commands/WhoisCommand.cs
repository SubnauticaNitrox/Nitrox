using System.ComponentModel;
using System.Text;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using NitroxModel.Dto;

namespace Nitrox.Server.Subnautica.Models.Commands;

internal class WhoisCommand : ICommandHandler<ConnectedPlayerDto>
{
    [Description("Shows information about a player")]
    public async Task Execute(ICommandContext context, [Description("The player to check out")] ConnectedPlayerDto player)
    {
        StringBuilder builder = new($"==== {player.Name} ====\n");
        builder.AppendLine($"Session ID: {player.SessionId}");
        builder.AppendLine($"Role: {player.Permissions}");
        // TODO: USE DATABASE
        // builder.AppendLine($"Position: {player.Position.X}, {player.Position.Y}, {player.Position.Z}");
        // builder.AppendLine($"Oxygen: {player.Stats.Oxygen}/{player.Stats.MaxOxygen}");
        // builder.AppendLine($"Food: {player.Stats.Food}");
        // builder.AppendLine($"Water: {player.Stats.Water}");
        // builder.AppendLine($"Infection: {player.Stats.InfectionAmount}");

        await context.ReplyAsync(builder.ToString());
    }
}

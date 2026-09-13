using System.ComponentModel;
using System.Text;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.PLAYER)]
internal sealed class WhoisCommand(PlayerService playerService) : ICommandHandler, ICommandHandler<SessionId>
{
    private readonly PlayerService playerService = playerService;

    [RequiresOrigin(CommandOrigin.PLAYER)]
    [Description("Shows information about the current player")]
    public async Task Execute(ICommandContext context)
    {
        if (context is not PlayerToServerCommandContext playerContext)
        {
            throw new Exception("Player context is required to run this command");
        }
        await Execute(context, playerContext.OriginId);
    }

    [Description("Shows information about a player")]
    public async Task Execute(ICommandContext context, SessionId targetPlayer)
    {
        StringBuilder builder = new($"==== {playerService.GetProperty<NameProperty>(targetPlayer).Value} ====\n");
        builder.AppendLine($"Id: {playerService.GetProperty<GameObjectIdProperty>(targetPlayer).Value}");
        builder.AppendLine($"SessionId: {targetPlayer}");
        builder.AppendLine($"Role: {playerService.GetProperty<PermissionsProperty>(targetPlayer).Value}");
        builder.AppendLine($"Gamemode: {playerService.GetProperty<GameModeProperty>(targetPlayer).Value}");
        NitroxVector3 position = playerService.GetProperty<PositionProperty>(targetPlayer).Value;
        builder.AppendLine($"Position: {position.X}, {position.Y}, {position.Z}");
        PlayerStatsData stats = playerService.GetProperty<StatsProperty>(targetPlayer).Value;
        builder.AppendLine($"Oxygen: {stats.Oxygen}/{stats.MaxOxygen}");
        builder.AppendLine($"Food: {stats.Food}");
        builder.AppendLine($"Water: {stats.Water}");
        builder.AppendLine($"Infection: {stats.InfectionAmount}");
        builder.AppendLine($"In precursor: {playerService.GetProperty<InPrecursorProperty>(targetPlayer).Value}");

        await context.ReplyAsync(builder.ToString());
    }
}

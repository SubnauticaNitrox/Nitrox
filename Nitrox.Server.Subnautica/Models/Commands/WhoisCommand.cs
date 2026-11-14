using System.ComponentModel;
using System.Text;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.PLAYER)]
internal sealed class WhoisCommand(IPlayerPropProvider playerPropProvider) : ICommandHandler, ICommandHandler<SessionId>
{
    private readonly IPlayerPropProvider playerPropProvider = playerPropProvider;

    [RequiresOrigin(CommandOrigin.PLAYER)]
    [Description("Shows information about the current player")]
    public async Task Execute(ICommandContext context)
    {
        if (context is not PlayerToServerCommandContext playerContext)
        {
            throw new InvalidOperationException("Player context is required to run this command");
        }
        await Execute(context, playerContext.OriginId);
    }

    [Description("Shows information about a player")]
    public async Task Execute(ICommandContext context, SessionId targetPlayer)
    {
        StringBuilder builder = new($"==== {playerPropProvider.Get<NameProperty>(targetPlayer).Value} ====\n");
        builder.AppendLine($"Id: {playerPropProvider.Get<GameObjectIdProperty>(targetPlayer).Value}");
        builder.AppendLine($"SessionId: {targetPlayer}");
        builder.AppendLine($"Role: {playerPropProvider.Get<PermissionsProperty>(targetPlayer).Value}");
        builder.AppendLine($"Gamemode: {playerPropProvider.Get<GameModeProperty>(targetPlayer).Value}");
        NitroxVector3 position = playerPropProvider.Get<PositionProperty>(targetPlayer).Value;
        builder.AppendLine($"Position: {position.X}, {position.Y}, {position.Z}");
        PlayerStatsData stats = playerPropProvider.Get<StatsProperty>(targetPlayer).Value;
        builder.AppendLine($"Oxygen: {stats.Oxygen}/{stats.MaxOxygen}");
        builder.AppendLine($"Food: {stats.Food}");
        builder.AppendLine($"Water: {stats.Water}");
        builder.AppendLine($"Infection: {stats.InfectionAmount}");
        builder.AppendLine($"In precursor: {playerPropProvider.Get<InPrecursorProperty>(targetPlayer).Value}");

        await context.ReplyAsync(builder.ToString());
    }
}

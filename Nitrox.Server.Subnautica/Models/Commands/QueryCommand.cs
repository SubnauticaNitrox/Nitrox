using System.ComponentModel;
using System.Text;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.ADMIN)]
internal sealed class QueryCommand(EntityRegistry entityRegistry, SimulationOwnershipData simulationOwnershipData, ILogger<QueryCommand> logger) : ICommandHandler<NitroxId>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;
    private readonly ILogger<QueryCommand> logger = logger;

    [Description("Query the entity associated with the given NitroxId")]
    public async Task Execute(ICommandContext context, [Description("NitroxId of an entity")] NitroxId entityId)
    {
        if (!entityRegistry.TryGetEntityById(entityId, out Entity entity))
        {
            await context.ReplyAsync($"Entity with id {entityId} not found");
            return;
        }

        StringBuilder builder = new();
        builder.AppendLine("Entity");
        builder.AppendLine($" └ Type: {entity.GetType().Name}");
        builder.AppendLine($" └ Id: {entity.Id}");
        builder.AppendLine($" └ TechType: {entity.TechType}");
        builder.AppendLine($" └ ParentId: {entity.ParentId?.ToString() ?? "none"}");
        builder.AppendLine($" └ Children: {entity.ChildEntities.Count}");
        builder.AppendLine($" └ Metadata: {entity.Metadata?.ToString() ?? "none"}");

        if (entity is WorldEntity worldEntity)
        {
            builder.AppendLine("World");
            builder.AppendLine($" └ ClassId: {worldEntity.ClassId}");
            builder.AppendLine($" └ Level: {worldEntity.Level}");
            builder.AppendLine($" └ SpawnedByServer: {worldEntity.SpawnedByServer}");
            builder.AppendLine($" └ {worldEntity.Transform}");
            builder.AppendLine($" └ Cell: {(worldEntity is GlobalRootEntity ? "global root" : worldEntity.AbsoluteEntityCell.ToString())}");
        }

        bool isLocked = simulationOwnershipData.TryGetLock(entityId, out SimulationOwnershipData.PlayerLock playerLock);

        builder.AppendLine("Lock status");
        builder.AppendLine($" └ Locked: {isLocked}");
        builder.AppendLine($" └ Owner: {(isLocked ? playerLock.Player.Name : "none")}");

        builder.AppendLine("Raw Data");
        builder.AppendLine(entity.ToString());

        await context.ReplyAsync(builder.ToString());
    }
}

using System.ComponentModel;
using System.Linq;
using System.Text;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.ADMIN)]
internal sealed class QueryCommand(EntityRegistry entityRegistry, SimulationOwnershipData simulationOwnershipData, PlayerService playerService) : ICommandHandler<NitroxId>, ICommandHandler<SessionId>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly PlayerService playerService = playerService;
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;

    [Description("Query the entity associated with the given NitroxId")]
    public async Task Execute(ICommandContext context, [Description("NitroxId of an entity")] NitroxId entityId)
    {
        if (!entityRegistry.TryGetEntityById(entityId, out Entity entity))
        {
            await context.ReplyAsync($"Entity with id {entityId} not found");
            return;
        }

        await SendEntityOverview(context, entity);
    }

    public async Task Execute(ICommandContext context, SessionId sessionId)
    {
        if (!entityRegistry.TryGetEntityById(playerService.GetProperty<GameObjectIdProperty>(sessionId).Value, out Entity entity))
        {
            await context.ReplyAsync($"No entity attached to player session #{sessionId}");
            return;
        }

        await SendEntityOverview(context, entity);
    }

    private async Task SendEntityOverview(ICommandContext context, Entity entity)
    {
        StringBuilder builder = new();
        builder.AppendLine("Entity");
        builder.AppendLine($" └ Type: {entity.GetType().Name}");
        builder.AppendLine($" └ Id: {entity.Id}");
        builder.AppendLine($" └ TechType: {entity.TechType}");
        builder.AppendLine($" └ ParentId: {entity.ParentId?.ToString() ?? "<null>"}");
        builder.AppendLine($" └ Metadata: {entity.Metadata?.ToString() ?? "<null>"}");
        builder.AppendLine($" └ Children: {entity.ChildEntities.Count}");
        if (entity.ChildEntities.Count > 0)
        {
            foreach (Entity childEntity in entity.ChildEntities)
            {
                builder.AppendLine("   └ Child");
                builder.AppendLine($"     └ Type: {childEntity.GetType().Name}");
                builder.AppendLine($"     └ Id: {childEntity.Id}");
                builder.AppendLine($"     └ TechType: {childEntity.TechType}");
                builder.AppendLine($"     └ Metadata: {childEntity.Metadata?.ToString() ?? "<null>"}");
                builder.AppendLine($"     └ Children: {childEntity.ChildEntities.Count}");
            }
        }

        if (entity is WorldEntity worldEntity)
        {
            builder.AppendLine("World");
            builder.AppendLine($" └ ClassId: {worldEntity.ClassId}");
            builder.AppendLine($" └ Level: {worldEntity.Level}");
            builder.AppendLine($" └ SpawnedByServer: {worldEntity.SpawnedByServer}");
            builder.AppendLine($" └ {worldEntity.Transform}");
            builder.AppendLine($" └ Cell: {(worldEntity is GlobalRootEntity ? "global root" : worldEntity.AbsoluteEntityCell.ToString())}");
        }

        if (entity is PlayerEntity)
        {
            (SessionId? serverPlayerId, GameObjectIdProperty? _) = playerService.GetProperties<GameObjectIdProperty>().FirstOrDefault(p => p.Item2.Value == entity.Id);
            if (serverPlayerId.HasValue)
            {
                builder.AppendLine("Player");
                builder.AppendLine($" └ Name: {playerService.GetProperty<NameProperty>(serverPlayerId.Value).Value}");
                builder.AppendLine($" └ Online: {playerService.GetProperty<IsOnlineProperty>(serverPlayerId.Value).Value}");
                builder.AppendLine($" └ Perms: {playerService.GetProperty<PermissionsProperty>(serverPlayerId.Value).Value}");
                builder.AppendLine($" └ GameMode: {playerService.GetProperty<GameModeProperty>(serverPlayerId.Value).Value}");
                builder.AppendLine($" └ InPrecursor: {playerService.GetProperty<InPrecursorProperty>(serverPlayerId.Value).Value}");
                builder.AppendLine($" └ Stats: {playerService.GetProperty<StatsProperty>(serverPlayerId.Value).Value}");
                builder.AppendLine($" └ DisplaySurfaceWater: {playerService.GetProperty<DisplaySurfaceWaterProperty>(serverPlayerId.Value).Value}");
                builder.AppendLine($" └ Position: {playerService.GetProperty<PositionProperty>(serverPlayerId.Value).Value}");
                builder.AppendLine($" └ LastStoredPosition: {playerService.GetProperty<CheckpointPositionProperty>(serverPlayerId.Value).Value.ToString()}");
                builder.AppendLine($" └ SubRootId: {playerService.GetProperty<SubRootIdProperty>(serverPlayerId.Value).Value?.ToString() ?? "<null>"}");
                builder.AppendLine($" └ LastStoredSubRootID: {playerService.GetProperty<CheckpointSubRootIdProperty>(serverPlayerId.Value).Value?.ToString() ?? "<null>"}");

                if (entity.ParentId != playerService.GetProperty<SubRootIdProperty>(serverPlayerId.Value).Value)
                {
                    builder.AppendLine("⚠ ParentId doesn't match SubRootId");
                }
            }
            else
            {
                builder.AppendLine("Player");
                builder.AppendLine("⚠ Unable to find player record for this entity id)");
            }
        }

        bool isLocked = simulationOwnershipData.TryGetLock(entity.Id, out SimulationOwnershipData.PlayerLock playerLock);

        builder.AppendLine("Lock status");
        builder.AppendLine($" └ Locked: {isLocked}");
        builder.AppendLine($" └ Owner: {(isLocked ? $"{playerService.GetProperty<NameProperty>(playerLock.SessionId).Value} #{playerLock.SessionId}" : "<null>")}");

        builder.AppendLine("Raw Data");
        builder.AppendLine(entity.ToString());

        await context.ReplyAsync(builder.ToString());
    }
}

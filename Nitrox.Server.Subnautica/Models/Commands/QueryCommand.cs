using System.ComponentModel;
using System.Linq;
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
internal sealed class QueryCommand(EntityRegistry entityRegistry, SimulationOwnershipData simulationOwnershipData, PlayerManager playerManager) : ICommandHandler<NitroxId>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly PlayerManager playerManager = playerManager;
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;

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
            Player? serverPlayer = playerManager.GetAllPlayers().FirstOrDefault(p => p.GameObjectId == entityId);
            if (serverPlayer is not null)
            {
                builder.AppendLine("Player");
                builder.AppendLine($" └ Name: {serverPlayer.Name}");
                builder.AppendLine($" └ Online: {serverPlayer.IsOnline}");
                builder.AppendLine($" └ Perms: {serverPlayer.Permissions}");
                builder.AppendLine($" └ GameMode: {serverPlayer.GameMode}");
                builder.AppendLine($" └ InPrecursor: {serverPlayer.InPrecursor}");
                builder.AppendLine($" └ Stats: {serverPlayer.Stats}");
                builder.AppendLine($" └ DisplaySurfaceWater: {serverPlayer.DisplaySurfaceWater}");
                builder.AppendLine($" └ Position: {serverPlayer.Position}");
                builder.AppendLine($" └ LastStoredPosition: {serverPlayer.LastStoredPosition?.ToString() ?? "<null>"}");
                builder.AppendLine($" └ SubRootId: {serverPlayer.SubRootId.OrNull()?.ToString() ?? "<null>"}");
                builder.AppendLine($" └ LastStoredSubRootID: {serverPlayer.LastStoredSubRootID.OrNull()?.ToString() ?? "<null>"}");

                if (entity.ParentId != serverPlayer.SubRootId.OrNull())
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

        bool isLocked = simulationOwnershipData.TryGetLock(entityId, out SimulationOwnershipData.PlayerLock playerLock);

        builder.AppendLine("Lock status");
        builder.AppendLine($" └ Locked: {isLocked}");
        builder.AppendLine($" └ Owner: {(isLocked ? $"{playerLock.Player.Name} #{playerLock.Player.SessionId}" : "<null>")}");

        builder.AppendLine("Raw Data");
        builder.AppendLine(entity.ToString());

        await context.ReplyAsync(builder.ToString());
    }
}

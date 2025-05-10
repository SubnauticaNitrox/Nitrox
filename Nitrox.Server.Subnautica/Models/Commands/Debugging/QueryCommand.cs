#if DEBUG
using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Commands.Debugging;

[RequiresPermission(Perms.SUPERADMIN)]
internal class QueryCommand(EntityRegistry entityRegistry, SimulationOwnershipData simulationOwnershipData) : ICommandHandler<NitroxId>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;

    [Description("Query the entity associated with the given NitroxId")]
    public async Task Execute(ICommandContext context, [Description("NitroxId of an entity")] NitroxId entityId)
    {
        if (!entityRegistry.TryGetEntityById(entityId, out Entity entity))
        {
            await context.ReplyAsync($"Entity with id {entityId} not found");
        }

        await context.ReplyAsync(entity.ToString());
        if (entity is WorldEntity { Transform: not null } worldEntity)
        {
            await context.ReplyAsync(worldEntity.AbsoluteEntityCell.ToString());
        }
        if (simulationOwnershipData.TryGetLock(entityId, out SimulationOwnershipData.PlayerLock playerLock))
        {
            await context.ReplyAsync($"Lock owner player id: {playerLock.PlayerId}");
        }
        else
        {
            await context.ReplyAsync("Not locked");
        }
    }
}
#endif

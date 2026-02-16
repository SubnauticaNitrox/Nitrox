#if DEBUG
using System.ComponentModel;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Commands.Debugging;

[RequiresPermission(Perms.HOST)]
internal sealed class QueryCommand(EntityRegistry entityRegistry, SimulationOwnershipData simulationOwnershipData, ILogger<QueryCommand> logger) : ICommandHandler<NitroxId>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;
    private readonly ILogger<QueryCommand> logger = logger;

    [Description("Query the entity associated with the given NitroxId")]
    public Task Execute(ICommandContext context, [Description("NitroxId of an entity")] NitroxId entityId)
    {
        if (!entityRegistry.TryGetEntityById(entityId, out Entity entity))
        {
            logger.ZLogError($"Entity with id {entityId} not found");
            return Task.CompletedTask;
        }

        logger.ZLogInformation($"{entity}");
        if (entity is WorldEntity worldEntity and not GlobalRootEntity)
        {
            logger.ZLogInformation($"{worldEntity.AbsoluteEntityCell}");
        }
        if (simulationOwnershipData.TryGetLock(entityId, out SimulationOwnershipData.PlayerLock playerLock))
        {
            logger.ZLogInformation($"Lock owner: {playerLock.Player.Name}");
        }
        else
        {
            logger.ZLogInformation($"Not locked");
        }

        return Task.CompletedTask;
    }
}
#endif

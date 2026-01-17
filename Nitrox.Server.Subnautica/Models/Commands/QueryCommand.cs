#if DEBUG
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Commands.Abstract.Type;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Commands;

internal class QueryCommand : Command
{
    private readonly EntityRegistry entityRegistry;
    private readonly SimulationOwnershipData simulationOwnershipData;
    private readonly ILogger<QueryCommand> logger;

    public QueryCommand(EntityRegistry entityRegistry, SimulationOwnershipData simulationOwnershipData, ILogger<QueryCommand> logger) : base("query", Perms.HOST, "Query the entity associated with the given NitroxId")
    {
        this.entityRegistry = entityRegistry;
        this.simulationOwnershipData = simulationOwnershipData;
        this.logger = logger;
        AddParameter(new TypeNitroxId("entityId", true, "NitroxId of the queried entity"));
    }

    protected override void Execute(CallArgs args)
    {
        NitroxId nitroxId = args.Get<NitroxId>(0);

        if (entityRegistry.TryGetEntityById(nitroxId, out Entity entity))
        {
            logger.ZLogInformation($"{entity}");
            if (entity is WorldEntity worldEntity && worldEntity.Transform != null && worldEntity is not GlobalRootEntity)
            {
                logger.ZLogInformation($"{worldEntity.AbsoluteEntityCell}");
            }
            if (simulationOwnershipData.TryGetLock(nitroxId, out SimulationOwnershipData.PlayerLock playerLock))
            {
                logger.ZLogInformation($"Lock owner: {playerLock.Player.Name}");
            }
            else
            {
                logger.ZLogInformation($"Not locked");
            }
        }
        else
        {
            logger.ZLogError($"Entity with id {nitroxId} not found");
        }
    }
}
#endif

#if DEBUG
using System;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.ConsoleCommands;

internal class QueryCommand : Command
{
    private readonly Lazy<EntityRegistry> entityRegistry = new(NitroxServiceLocator.LocateService<EntityRegistry>);
    private readonly Lazy<SimulationOwnershipData> simulationOwnershipData = new(NitroxServiceLocator.LocateService<SimulationOwnershipData>);

    public QueryCommand() : base("query", Perms.CONSOLE, "Query the entity associated with the given NitroxId")
    {
        AddParameter(new TypeNitroxId("entityId", true, "NitroxId of the queried entity"));
    }

    protected override void Execute(CallArgs args)
    {
        NitroxId nitroxId = args.Get<NitroxId>(0);

        if (entityRegistry.Value.TryGetEntityById(nitroxId, out Entity entity))
        {
            Log.Info(entity);
            if (entity is WorldEntity worldEntity && worldEntity.Transform != null)
            {
                Log.Info(worldEntity.AbsoluteEntityCell);
            }
            if (simulationOwnershipData.Value.TryGetLock(nitroxId, out SimulationOwnershipData.PlayerLock playerLock))
            {
                Log.Info($"Lock owner: {playerLock.Player.Name}");
            }
            else
            {
                Log.Info("Not locked");
            }
        }
        else
        {
            Log.Error($"Entity with id {nitroxId} not found");
        }
    }
}
#endif

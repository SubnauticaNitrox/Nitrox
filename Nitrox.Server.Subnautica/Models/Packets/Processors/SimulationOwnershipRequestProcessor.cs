using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SimulationOwnershipRequestProcessor(EntitySimulation entitySimulation, EntityRegistry entityRegistry) : IAuthPacketProcessor<SimulationOwnershipRequest>
{
    private readonly EntitySimulation entitySimulation = entitySimulation;
    private readonly EntityRegistry entityRegistry = entityRegistry;

    public async Task Process(AuthProcessorContext context, SimulationOwnershipRequest ownershipRequest)
    {
        SimulationOwnershipResponse responseToPlayer;
        if (!entityRegistry.TryGetEntityById(ownershipRequest.Id, out WorldEntity worldEntity))
        {
            responseToPlayer = new(ownershipRequest.Id, false, ownershipRequest.LockType);
            await context.ReplyAsync(responseToPlayer);
            return;
        }

        List<SimulatedEntity> simulatedEntities = [.. entitySimulation.TryAcquireEntity(context.Sender, worldEntity, true, ownershipRequest.LockType)];

        if (simulatedEntities.Count == 0)
        {
            responseToPlayer = new(ownershipRequest.Id, false, ownershipRequest.LockType);
            await context.ReplyAsync(responseToPlayer);
            return;
        }

        responseToPlayer = new(ownershipRequest.Id, true, ownershipRequest.LockType);
        await context.ReplyAsync(responseToPlayer);

        SimulationOwnershipChange simulationOwnershipChange = new(simulatedEntities);
        await context.SendToAllAsync(simulationOwnershipChange);
    }
}

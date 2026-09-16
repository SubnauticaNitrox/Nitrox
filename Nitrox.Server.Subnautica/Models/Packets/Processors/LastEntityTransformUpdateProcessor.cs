using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using static Nitrox.Model.Subnautica.Packets.EntityTransformUpdates;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class LastEntityTransformUpdateProcessor(WorldEntityManager worldEntityManager, SimulationOwnershipData simulationOwnershipData, PlayerManager playerManager, EntitySimulation entitySimulation) : IAuthPacketProcessor<LastEntityTransformUpdate>
{
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;
    private readonly PlayerManager playerManager = playerManager;
    private readonly EntitySimulation entitySimulation = entitySimulation;

    public async Task Process(AuthProcessorContext context, LastEntityTransformUpdate packet)
    {
        EntityTransformUpdate lastUpdate = packet.LastUpdate;
        if (!simulationOwnershipData.RevokeIfOwner(lastUpdate.Id, context.Sender))
        {
            return;
        }

        if (!worldEntityManager.TryUpdateEntityPosition(lastUpdate.Id, lastUpdate.Position, lastUpdate.Rotation, out AbsoluteEntityCell currentCell, out WorldEntity worldEntity))
        {
            return;
        }

        EntityTransformUpdates entityTransformUpdates = new([lastUpdate]);

        foreach (Player player in playerManager.GetConnectedPlayersExcept(context.Sender.SessionId))
        {
            if (player.CanSee(worldEntity))
            {
                await context.SendAsync(entityTransformUpdates, player.SessionId);
            }
        }

        List<SimulatedEntity> simulationChange = [];
        entitySimulation.AssignEntitiesToOtherPlayers(context.Sender.SessionId, [worldEntity], simulationChange);

        await context.SendToAllAsync(new SimulationOwnershipChange(simulationChange));
    }
}

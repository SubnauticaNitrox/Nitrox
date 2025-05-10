using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SimulationOwnershipRequestProcessor(IServerPacketSender packetSender, SimulationOwnershipData simulationOwnershipData, EntitySimulation entitySimulation) : IAuthPacketProcessor<SimulationOwnershipRequest>
{
    private readonly IServerPacketSender packetSender = packetSender;
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;
    private readonly EntitySimulation entitySimulation = entitySimulation;

    public async Task Process(AuthProcessorContext context, SimulationOwnershipRequest ownershipRequest)
    {
        bool acquiredLock = simulationOwnershipData.TryToAcquire(ownershipRequest.Id, context.Sender.PlayerId, ownershipRequest.LockType);
        if (acquiredLock)
        {
            bool shouldEntityMove = entitySimulation.ShouldSimulateEntityMovement(ownershipRequest.Id);
            SimulationOwnershipChange simulationOwnershipChange = new(ownershipRequest.Id, context.Sender.PlayerId, ownershipRequest.LockType, shouldEntityMove);
            await packetSender.SendPacketToOthers(simulationOwnershipChange, context.Sender.SessionId);
        }

        await context.ReplyToSender(new SimulationOwnershipResponse(ownershipRequest.Id, acquiredLock, ownershipRequest.LockType));
    }
}

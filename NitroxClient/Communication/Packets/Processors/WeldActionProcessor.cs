using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal class WeldActionProcessor(SimulationOwnership simulationOwnership) : IClientPacketProcessor<WeldAction>
{
    private readonly SimulationOwnership simulationOwnership = simulationOwnership;

    public Task Process(ClientProcessorContext context, WeldAction packet)
    {
        GameObject gameObject = NitroxEntity.RequireObjectFrom(packet.Id);

        if (!simulationOwnership.HasAnyLockType(packet.Id))
        {
            Log.Error($"Got WeldAction packet for {packet.Id} but did not find the lock corresponding to it");
            return Task.CompletedTask;
        }

        LiveMixin liveMixin = gameObject.GetComponent<LiveMixin>();
        if (!liveMixin)
        {
            Log.Error($"Did not find LiveMixin for GameObject {packet.Id} even though it was welded.");
            return Task.CompletedTask;
        }
        // If we add other player sounds/animations, this is the place to do it for welding
        liveMixin.AddHealth(packet.HealthAdded);
        return Task.CompletedTask;
    }
}

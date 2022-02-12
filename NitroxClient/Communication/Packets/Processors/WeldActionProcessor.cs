using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class WeldActionProcessor : ClientPacketProcessor<WeldAction>
    {
        private IMultiplayerSession multiplayerSession;
        private SimulationOwnership simulationOwnership;

        public WeldActionProcessor(IMultiplayerSession multiplayerSession, SimulationOwnership simulationOwnership)
        {
            this.multiplayerSession = multiplayerSession;
            this.simulationOwnership = simulationOwnership;
        }

        public override void Process(WeldAction packet)
        {
            GameObject gameObject = NitroxEntity.RequireObjectFrom(packet.Id);

            if (!simulationOwnership.HasAnyLockType(packet.Id))
            {
                Log.Error($"Got WeldAction packet for {packet.Id} but did not find the lock corresponding to it");
                return;
            }

            LiveMixin liveMixin = gameObject.GetComponent<LiveMixin>();
            if (!liveMixin)
            {
                Log.Error($"Did not find LiveMixin for GameObject {packet.Id} even though it was welded.");
                return;
            }
            // If we add other player sounds/animations, this is the place to do it for welding
            liveMixin.AddHealth(packet.HealthAdded);
        }
    }
}

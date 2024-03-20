using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using UnityEngine;
using static NitroxModel.DisplayStatusCodes;
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
                DisplayStatusCode(StatusCode.LOCK_ERR, $"Got WeldAction packet for {packet.Id} but did not find the lock corresponding to it");
                return;
            }

            LiveMixin liveMixin = gameObject.GetComponent<LiveMixin>();
            if (!liveMixin)
            {
                DisplayStatusCode(StatusCode.INVALID_VARIABLE_VAL, $"Did not find LiveMixin for GameObject {packet.Id} even though it was welded.");
                return;
            }
            // If we add other player sounds/animations, this is the place to do it for welding
            liveMixin.AddHealth(packet.HealthAdded);
        }
    }
}

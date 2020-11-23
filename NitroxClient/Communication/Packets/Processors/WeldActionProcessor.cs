using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
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
            Optional<GameObject> opGameObject = NitroxEntity.GetObjectFrom(packet.Id);
            if (opGameObject.HasValue)
            {
                if (simulationOwnership.HasAnyLockType(packet.Id))
                {
                    GameObject gameObject = opGameObject.Value;
                    LiveMixin liveMixin = gameObject.GetComponent<LiveMixin>();
                    if (liveMixin)
                    {
                        liveMixin.AddHealth(packet.HealthAdded);
                    }
                    else
                    {
                        Log.Error($"Did not find LiveMixin for GameObject {packet.Id} even though it was welded.");
                    }
                }
                else
                {
                    Log.Error($"Got WeldAction packet for {packet.Id} but did not find the lock corresponding to it");
                }
            }
            else
            {
                Log.Error($"Did not find GameObject {packet.Id} for WeldAction even though this player should have the simulation lock");
            }
        }
    }
}

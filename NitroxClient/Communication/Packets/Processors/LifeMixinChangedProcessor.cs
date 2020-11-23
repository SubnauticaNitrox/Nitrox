using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel_Subnautica.Helper;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class LifeMixinChangedProcessor : ClientPacketProcessor<LiveMixinHealthChanged>
    {
        private readonly IPacketSender packetSender;
        private readonly SimulationOwnership simulationOwnership;
        public LifeMixinChangedProcessor(IPacketSender packetSender, SimulationOwnership simulationOwnership)
        {
            this.packetSender = packetSender;
            this.simulationOwnership = simulationOwnership;

        }
        public override void Process(LiveMixinHealthChanged packet)
        {
            if (simulationOwnership.HasAnyLockType(packet.Id))
            {
                Log.Error($"Got LiveMixin change health for {packet.Id} but we have the simulation already. This should not happen!");
            }
            else            
            {
                if (!simulationOwnership.OtherPlayerHasAnyLock(packet.Id))
                {
                    Log.Error($"Got LiveMixin change health for {packet.Id} but found no simulation at all. Simulation lock for unknown id will be added.");
                }
                Optional<GameObject> opGameObject = NitroxEntity.GetObjectFrom(packet.Id);
                if (opGameObject.HasValue)
                {
                    GameObject gameObject = opGameObject.Value;

                    LiveMixin liveMixin = opGameObject.Value.GetComponent<LiveMixin>();
                    // Since this is send by the simulator (presumably)
                    simulationOwnership.AddSimulationOverride(packet.Id);
                    if (packet.LifeChanged < 0)
                    {
                        Optional<GameObject> opDealer = packet.DealerId.HasValue ? NitroxEntity.GetObjectFrom(packet.DealerId.Value) : Optional.Empty;
                        GameObject dealer = opDealer.HasValue ? opDealer.Value : null;
                        if (dealer == null && packet.DealerId.HasValue)
                        {
                            Log.Warn($"Could not find entity {packet.DealerId.Value}. This can lead to problems.");
                        }
                        liveMixin.TakeDamage(-packet.LifeChanged, packet.Position.ToUnity(), (DamageType)packet.Damagetype, dealer);
                    }
                    else
                    {
                        liveMixin.AddHealth(packet.LifeChanged);
                    }
                    
                    // Check if the health calculated by the game is the same as the calculated damage from the simulator
                    if (liveMixin.health != packet.TotalHealth)
                    {
                        Log.Warn($"Calculated health and send health for {packet.Id} do not align (Calculated: {liveMixin.health}, send:{packet.TotalHealth}). This will be correted but should be investigated");
                        liveMixin.health = packet.TotalHealth;
                    }
                }
            }
        }
    }
}

using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Client.Unity.Helper;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;
using UnityEngine;

namespace Nitrox.Client.Communication.Packets.Processors
{
    public class EnergyMixinValueChangedProcessor : ClientPacketProcessor<EnergyMixinValueChanged>
    {
        private readonly IPacketSender packetSender;

        public EnergyMixinValueChangedProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(EnergyMixinValueChanged energyMixinPacket)
        {
            Optional<GameObject> target = NitroxEntity.GetObjectFrom(energyMixinPacket.Id); // Can't find inventory items for now because they aren't really synced.
            if (target.HasValue)
            {
                EnergyMixin energyMixin = target.Value.RequireComponent<EnergyMixin>();

                using (packetSender.Suppress<EnergyMixinValueChanged>())
                {
                    energyMixin.ModifyCharge(energyMixinPacket.Value - energyMixin.charge);
                }
            }
        }
    }
}

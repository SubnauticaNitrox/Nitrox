using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
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
            GameObject target = NitroxEntity.RequireObjectFrom(energyMixinPacket.Id);
            EnergyMixin energyMixin = target.RequireComponent<EnergyMixin>();

            using (packetSender.Suppress<EnergyMixinValueChanged>())
            {
                energyMixin.ModifyCharge(energyMixinPacket.Value - energyMixin.charge);
            }
        }
    }
}

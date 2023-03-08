using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.Util;
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
            Optional<GameObject> target = NitroxEntity.GetObjectFrom(energyMixinPacket.OwnerId); // Can't find inventory items for now because they aren't really synced.
            if (target.HasValue)
            {
                EnergyMixin energyMixin = target.Value.RequireComponent<EnergyMixin>();

                using (PacketSuppressor<EnergyMixinValueChanged>.Suppress())
                {
                    energyMixin.ModifyCharge(energyMixinPacket.Value - energyMixin.charge);
                }
            }
        }
    }
}

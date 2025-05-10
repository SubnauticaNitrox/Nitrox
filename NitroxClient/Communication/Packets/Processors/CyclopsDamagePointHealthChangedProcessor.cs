using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsDamagePointHealthChangedProcessor : IClientPacketProcessor<CyclopsDamagePointRepaired>
    {
        private readonly IPacketSender packetSender;

        public CyclopsDamagePointHealthChangedProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public Task Process(IPacketProcessContext context, CyclopsDamagePointRepaired packet)
        {
            GameObject gameObject = NitroxEntity.RequireObjectFrom(packet.Id);
            SubRoot cyclops = gameObject.RequireComponent<SubRoot>();

            using (PacketSuppressor<CyclopsDamage>.Suppress())
            using (PacketSuppressor<CyclopsDamagePointRepaired>.Suppress())
            {
                cyclops.damageManager.damagePoints[packet.DamagePointIndex].liveMixin.AddHealth(packet.RepairAmount);
            }

            return Task.CompletedTask;
        }
    }
}

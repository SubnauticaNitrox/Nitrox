using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsDamagePointHealthChangedProcessor : ClientPacketProcessor<CyclopsDamagePointRepaired>
    {
        private readonly IPacketSender packetSender;

        public CyclopsDamagePointHealthChangedProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsDamagePointRepaired packet)
        {
            GameObject gameObject = GuidHelper.RequireObjectFrom(packet.Guid);
            SubRoot cyclops = GameObjectHelper.RequireComponent<SubRoot>(gameObject);

            using (packetSender.Suppress<CyclopsDamage>())
            {
                using (packetSender.Suppress<CyclopsDamagePointRepaired>())
                {
                    cyclops.damageManager.damagePoints[packet.DamagePointIndex].liveMixin.AddHealth(packet.RepairAmount);
                }
            }
        }
    }
}

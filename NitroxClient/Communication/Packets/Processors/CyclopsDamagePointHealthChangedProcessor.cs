using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsDamagePointHealthChangedProcessor : ClientPacketProcessor<CyclopsDamagePointHealthChanged>
    {
        private readonly IPacketSender packetSender;

        public CyclopsDamagePointHealthChangedProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsDamagePointHealthChanged packet)
        {
            GameObject gameObject = GuidHelper.RequireObjectFrom(packet.Guid);
            SubRoot cyclops = GameObjectHelper.RequireComponent<SubRoot>(gameObject);
            CyclopsDamagePoint damagePoint = cyclops.damageManager.damagePoints[packet.DamagePointIndex];

            using (packetSender.Suppress<CyclopsDamagePointHealthChanged>())
            {
                NitroxServiceLocator.LocateService<Cyclops>().OnDamagePointHealthChanged(cyclops, damagePoint, packet.RepairAmount, true);
            }
        }
    }
}

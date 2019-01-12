using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Core;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsDamageProcessor : ClientPacketProcessor<CyclopsDamage>
    {
        private readonly IPacketSender packetSender;

        public CyclopsDamageProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsDamage packet)
        {
            SubRoot cyclops = GuidHelper.RequireObjectFrom(packet.Guid).GetComponent<SubRoot>();

            using (packetSender.Suppress<CyclopsDamage>())
            {
                using (packetSender.Suppress<CyclopsDamagePointHealthChanged>())
                {
                    NitroxServiceLocator.LocateService<Cyclops>().SetActiveDamagePoints(cyclops, packet.DamagePointIndexes,
                        packet.SubHealth,
                        packet.DamageManagerHealth,
                        packet.SubFireHealth);
                }

                using (packetSender.Suppress<CyclopsFireHealthChanged>())
                {
                    NitroxServiceLocator.LocateService<Cyclops>().SetActiveRoomFires(cyclops, packet.RoomFires,
                    packet.SubHealth,
                    packet.DamageManagerHealth,
                    packet.SubFireHealth);
                }
            }
        }
    }
}

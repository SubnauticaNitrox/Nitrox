using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel_Subnautica.Helper;

namespace NitroxClient.Communication.Packets.Processors
{
    public class KnownTechEntryProcessorAdd : ClientPacketProcessor<KnownTechEntryAdd>
    {
        private readonly IPacketSender packetSender;

        public KnownTechEntryProcessorAdd(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(KnownTechEntryAdd packet)
        {
            using (packetSender.Suppress<KnownTechEntryAdd>())
            {
                KnownTech.Add(packet.TechType.ToUnity(), packet.Verbose);
            }
        }
    }
}

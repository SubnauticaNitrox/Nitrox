using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures;

namespace Nitrox.Client.Communication.Packets.Processors
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

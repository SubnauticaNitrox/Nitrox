using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PDAEncyclopediaEntryAddProcessor : ClientPacketProcessor<PDAEncyclopediaEntryAdd>
    {
        private readonly IPacketSender packetSender;

        public PDAEncyclopediaEntryAddProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(PDAEncyclopediaEntryAdd packet)
        {
            using (packetSender.Suppress<PDAEncyclopediaEntryAdd>())
            {
                PDAEncyclopedia.Add(packet.Key,true);
            }
        }
    }
}

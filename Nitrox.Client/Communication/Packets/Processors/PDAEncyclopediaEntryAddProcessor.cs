using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Model.Packets;

namespace Nitrox.Client.Communication.Packets.Processors
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

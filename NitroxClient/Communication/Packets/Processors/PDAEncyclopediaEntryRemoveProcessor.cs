using NitroxModel.Packets;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.Communication.Abstract;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PDAEncyclopediaEntryRemoveProcessor : ClientPacketProcessor<PDAEncyclopediaEntryRemove>
    {
        private readonly IPacketSender packetSender;

        public PDAEncyclopediaEntryRemoveProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(PDAEncyclopediaEntryRemove packet)
        {
            using (packetSender.Suppress<PDAEncyclopediaEntryRemove>())
            {
                PDAEncyclopedia.RemoveTimeCapsule(packet.Entry.Key);
            }
        }
    }
}

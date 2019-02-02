using NitroxModel.Packets;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.Communication.Abstract;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PDAEncyclopdiaEntryUpdateProcessor : ClientPacketProcessor<PDAEncyclopediaEntryUpdate>
    {
        private readonly IPacketSender packetSender;

        public PDAEncyclopdiaEntryUpdateProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(PDAEncyclopediaEntryUpdate packet)
        {
            using (packetSender.Suppress<PDAEncyclopediaEntryUpdate>())
            {
                PDAEncyclopedia.UpdateTimeCapsule(packet.Entry.Key);
            }
        }
    }
}

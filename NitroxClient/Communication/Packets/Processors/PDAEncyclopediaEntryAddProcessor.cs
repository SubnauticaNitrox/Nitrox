using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PDAEncyclopediaEntryAddProcessor : ClientPacketProcessor<PDAEncyclopediaEntryAdd>
    {
        public override void Process(PDAEncyclopediaEntryAdd packet)
        {
            PDAEncyclopedia.Add(packet.Key, true);
        }
    }
}

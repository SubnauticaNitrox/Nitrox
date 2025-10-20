using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class PDAEncyclopediaEntryAddProcessor : ClientPacketProcessor<PDAEncyclopediaEntryAdd>
{
    private readonly IPacketSender packetSender;

    public PDAEncyclopediaEntryAddProcessor(IPacketSender packetSender)
    {
        this.packetSender = packetSender;
    }

    public override void Process(PDAEncyclopediaEntryAdd packet)
    {
        using (PacketSuppressor<PDAEncyclopediaEntryAdd>.Suppress())
        {
            PDAEncyclopedia.Add(packet.Key, packet.Verbose);
        }
    }
}

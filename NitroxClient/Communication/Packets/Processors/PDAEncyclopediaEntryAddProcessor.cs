using NitroxClient.Communication.Abstract;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class PDAEncyclopediaEntryAddProcessor : IClientPacketProcessor<PdaEncyclopediaEntryAdd>
{
    private readonly IPacketSender packetSender;

    public PDAEncyclopediaEntryAddProcessor(IPacketSender packetSender)
    {
        this.packetSender = packetSender;
    }

    public Task Process(IPacketProcessContext context, PdaEncyclopediaEntryAdd packet)
    {
        using (PacketSuppressor<PdaEncyclopediaEntryAdd>.Suppress())
        {
            PDAEncyclopedia.Add(packet.Key, packet.Verbose);
        }

        return Task.CompletedTask;
    }
}

using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Core;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class PDAEncyclopediaEntryAddProcessor : IClientPacketProcessor<PDAEncyclopediaEntryAdd>
{
    public Task Process(ClientProcessorContext context, PDAEncyclopediaEntryAdd packet)
    {
        using (PacketSuppressor<PDAEncyclopediaEntryAdd>.Suppress())
        {
            PDAEncyclopedia.Add(packet.Key, packet.Verbose);
        }
        return Task.CompletedTask;
    }
}

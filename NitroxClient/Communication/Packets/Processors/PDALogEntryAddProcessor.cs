using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Core;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class PDALogEntryAddProcessor : IClientPacketProcessor<PDALogEntryAdd>
{
    public Task Process(ClientProcessorContext context, PDALogEntryAdd packet)
    {
        using (PacketSuppressor<PDALogEntryAdd>.Suppress())
        {
            PDALog.Add(packet.Key);
        }
        return Task.CompletedTask;
    }
}

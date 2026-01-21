using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class CyclopsFireCreatedProcessor(Fires fires) : IClientPacketProcessor<CyclopsFireCreated>
{
    private readonly Fires fires = fires;

    public Task Process(ClientProcessorContext context, CyclopsFireCreated packet)
    {
        fires.Create(packet.FireCreatedData);
        return Task.CompletedTask;
    }
}

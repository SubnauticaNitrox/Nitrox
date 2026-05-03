using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class FireCreatedProcessor(Fires fires) : IClientPacketProcessor<FireCreated>
{
    private readonly Fires fires = fires;

    public Task Process(ClientProcessorContext context, FireCreated packet)
    {
        fires.Create(packet.FireCreatedData);
        return Task.CompletedTask;
    }
}

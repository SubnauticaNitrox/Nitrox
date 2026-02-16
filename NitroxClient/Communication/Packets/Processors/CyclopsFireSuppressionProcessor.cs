using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class CyclopsFireSuppressionProcessor(Cyclops cyclops) : IClientPacketProcessor<CyclopsFireSuppression>
{
    private readonly Cyclops cyclops = cyclops;

    public Task Process(ClientProcessorContext context, CyclopsFireSuppression fireSuppressionPacket)
    {
        cyclops.StartFireSuppression(fireSuppressionPacket.Id);
        return Task.CompletedTask;
    }
}

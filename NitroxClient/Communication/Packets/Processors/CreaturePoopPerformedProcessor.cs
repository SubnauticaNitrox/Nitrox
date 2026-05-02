using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class CreaturePoopPerformedProcessor : IClientPacketProcessor<CreaturePoopPerformed>
{
    public Task Process(ClientProcessorContext context, CreaturePoopPerformed packet)
    {
        AI.CreaturePoopPerformed(packet.CreatureId);
        return Task.CompletedTask;
    }
}

using NitroxClient.GameLogic;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class CreaturePoopPerformedProcessor : IClientPacketProcessor<CreaturePoopPerformed>
{
    public Task Process(IPacketProcessContext context, CreaturePoopPerformed packet)
    {
        AI.CreaturePoopPerformed(packet.CreatureId);

        return Task.CompletedTask;
    }
}

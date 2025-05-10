using NitroxClient.GameLogic;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class CreatureActionProcessor : IClientPacketProcessor<CreatureActionChanged>
{
    private readonly AI ai;

    public CreatureActionProcessor(AI ai)
    {
        this.ai = ai;
    }

    public Task Process(IPacketProcessContext context, CreatureActionChanged packet)
    {
        ai.CreatureActionChanged(packet.CreatureId, packet.CreatureActionType);

        return Task.CompletedTask;
    }
}

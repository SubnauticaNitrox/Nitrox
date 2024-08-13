using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class CreatureActionProcessor : ClientPacketProcessor<CreatureActionChanged>
{
    private readonly AI ai;

    public CreatureActionProcessor(AI ai)
    {
        this.ai = ai;
    }

    public override void Process(CreatureActionChanged packet)
    {
        ai.CreatureActionChanged(packet.CreatureId, packet.CreatureActionType);
    }
}

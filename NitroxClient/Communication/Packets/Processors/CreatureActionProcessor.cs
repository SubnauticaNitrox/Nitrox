using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class CreatureActionProcessor(AI ai) : IClientPacketProcessor<CreatureActionChanged>
{
    private readonly AI ai = ai;

    public Task Process(ClientProcessorContext context, CreatureActionChanged packet)
    {
        ai.CreatureActionChanged(packet.CreatureId, packet.CreatureActionType);
        return Task.CompletedTask;
    }
}

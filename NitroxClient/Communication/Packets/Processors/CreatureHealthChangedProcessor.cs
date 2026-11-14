using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class CreatureHealthChangedProcessor(CreatureHealthManager creatureHealthManager) : IClientPacketProcessor<CreatureHealthChanged>
{
    private readonly CreatureHealthManager creatureHealthManager = creatureHealthManager;

    public Task Process(ClientProcessorContext context, CreatureHealthChanged packet)
    {
        creatureHealthManager.ApplyRemoteHealth(packet);
        return Task.CompletedTask;
    }
}

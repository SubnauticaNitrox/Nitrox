using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

/// <summary>
///     Applies another player's hit on a creature we simulate, or our own hit sent back by the server when nobody simulates the creature.
/// </summary>
internal sealed class CreatureDamageActionProcessor(CreatureHealthManager creatureHealthManager) : IClientPacketProcessor<CreatureDamageAction>
{
    private readonly CreatureHealthManager creatureHealthManager = creatureHealthManager;

    public Task Process(ClientProcessorContext context, CreatureDamageAction packet)
    {
        creatureHealthManager.ReceiveForwardedHit(packet);
        return Task.CompletedTask;
    }
}

using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

/// <summary>
///     Sends a player's hit on a creature to the creature's simulating player, the only one changing its health.
///     When nobody simulates it, the hit goes back to the sender who applies it like before.
/// </summary>
internal sealed class CreatureDamageActionProcessor(SimulationOwnershipData simulationOwnershipData, ILogger<CreatureDamageActionProcessor> logger) : IAuthPacketProcessor<CreatureDamageAction>
{
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;
    private readonly ILogger<CreatureDamageActionProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, CreatureDamageAction packet)
    {
        // Negative damage would heal the creature through LiveMixin.TakeDamage (NaN fails the comparison)
        if (!(packet.OriginalDamage > 0f) || float.IsInfinity(packet.OriginalDamage) || packet.Hops > CreatureDamageAction.MAX_HOPS)
        {
            return;
        }

        Player? simulatingPlayer = simulationOwnershipData.GetPlayerForLock(packet.CreatureId);
        if (simulatingPlayer == null)
        {
            // A re-routed hit comes from a player who doesn't have the creature anymore, nobody is left to apply it
            if (packet.Hops > 0)
            {
                logger.ZLogDebug($"Dropping re-routed hit of {packet.OriginalDamage} on creature {packet.CreatureId}: nobody simulates it");
                return;
            }

            // Nobody simulates it (e.g. a creature the server doesn't simulate): the attacker applies its own hit like before
            logger.ZLogDebug($"Nobody simulates creature {packet.CreatureId}, sending the hit of {packet.OriginalDamage} back to {context.Sender.Name}");
            await context.ReplyAsync(packet);
            return;
        }

        logger.ZLogTrace($"Sending hit of {packet.OriginalDamage} ({packet.Type}) on creature {packet.CreatureId} from {context.Sender.Name} to simulating player {simulatingPlayer.Name}");
        // Also covers the sender being the simulating player without knowing it yet: its SimulationOwnershipChange went out earlier on the same ordered channel
        await context.SendAsync(packet, simulatingPlayer.SessionId);
    }
}

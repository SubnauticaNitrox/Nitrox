using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

/// <summary>
///     Shares the health of a creature reported by its simulating player with the players who can see it, who are the possible next simulating players.
/// </summary>
internal sealed class CreatureHealthChangedProcessor(SimulationOwnershipData simulationOwnershipData, PlayerManager playerManager, EntityRegistry entityRegistry)
    : TransmitIfCanSeePacketProcessor<CreatureHealthChanged>(playerManager, entityRegistry)
{
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;

    public override async Task Process(AuthProcessorContext context, CreatureHealthChanged packet)
    {
        if (!IsReportFromSimulatingPlayer(packet, simulationOwnershipData.GetPlayerForLock(packet.CreatureId), context.Sender))
        {
            return;
        }

        // Also drops reports on dead creatures, which are no longer registered
        await TransmitIfCanSeeEntitiesAsync(context, packet, [packet.CreatureId]);
    }

    /// <summary>
    ///     Only the simulating player's health is real: this drops late (throttled) reports of a previous simulating player.
    /// </summary>
    internal static bool IsReportFromSimulatingPlayer(CreatureHealthChanged packet, Player? simulatingPlayer, Player sender)
    {
        // Receivers write the value directly, deaths are replicated by RemoveCreatureCorpse and EntityDestroyed
        if (!(packet.Health > 0f) || float.IsInfinity(packet.Health))
        {
            return false;
        }

        // Compared by session: Player's == compares the PeerId
        return simulatingPlayer != null && simulatingPlayer.SessionId == sender.SessionId;
    }
}

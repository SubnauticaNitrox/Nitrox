using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class RemoveCreatureCorpseProcessor(IPacketSender packetSender, PlayerManager playerManager, EntitySimulation entitySimulation, WorldEntityManager worldEntityManager) : IAuthPacketProcessor<RemoveCreatureCorpse>
{
    private readonly IPacketSender packetSender = packetSender;
    private readonly PlayerManager playerManager = playerManager;
    private readonly EntitySimulation entitySimulation = entitySimulation;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;

    public async Task Process(AuthProcessorContext context, RemoveCreatureCorpse packet)
    {
        entitySimulation.EntityDestroyed(packet.CreatureId);

        if (worldEntityManager.TryDestroyEntity(packet.CreatureId, out Entity entity))
        {
            foreach (Player player in playerManager.GetConnectedPlayers())
            {
                player.OutOfCellVisibleEntities.Remove(entity.Id);
            }

            // Not filtered by visibility because the server position of a creature can be outdated
            // and a player who still has it loaded would then keep it alive
            await context.SendToOthersAsync(packet);
        }
    }
}

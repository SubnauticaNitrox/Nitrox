using Nitrox.Model.Core;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class RemoveCreatureCorpseProcessor(PlayerService playerService, EntitySimulation entitySimulation, WorldEntityManager worldEntityManager) : IAuthPacketProcessor<RemoveCreatureCorpse>
{
    private readonly PlayerService playerService = playerService;
    private readonly EntitySimulation entitySimulation = entitySimulation;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;

    public async Task Process(AuthProcessorContext context, RemoveCreatureCorpse packet)
    {
        entitySimulation.EntityDestroyed(packet.CreatureId);

        if (worldEntityManager.TryDestroyEntity(packet.CreatureId, out Entity entity))
        {
            foreach (SessionId player in playerService.GetSessionIds())
            {
                bool isOtherPlayer = player != context.Sender;
                if (isOtherPlayer && playerService.GetProperty<VisibleCellsProperty>(player).CanSee(entity))
                {
                    playerService.GetProperty<OutOfCellVisibleEntitiesProperty>(player).Value.Remove(entity.Id);
                    await context.SendAsync(packet, player);
                }
            }
        }
    }
}

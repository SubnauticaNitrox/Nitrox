using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.Respositories;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class EntityDestroyedPacketProcessor(PlayerRepository playerRepository, EntitySimulation entitySimulation, WorldEntityManager worldEntityManager) : IAuthPacketProcessor<EntityDestroyed>
{
    private readonly PlayerRepository playerRepository = playerRepository;
    private readonly EntitySimulation entitySimulation = entitySimulation;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;

    public async Task Process(AuthProcessorContext context, EntityDestroyed packet)
    {
        entitySimulation.EntityDestroyed(packet.Id);

        if (worldEntityManager.TryDestroyEntity(packet.Id, out Entity entity))
        {
            if (entity is VehicleWorldEntity vehicleWorldEntity)
            {
                worldEntityManager.MovePlayerChildrenToRoot(vehicleWorldEntity);
            }

            // TODO: FIX THIS WITH THE DATABASE!
            // foreach (PeerId player in playerManager.GetConnectedPlayersAsync())
            // {
            //     bool isOtherPlayer = player != context.Sender;
            //     if (isOtherPlayer && player.CanSee(entity))
            //     {
            //          playerService.SendPacket(packet, player);
            //     }
            // }
        }
    }
}

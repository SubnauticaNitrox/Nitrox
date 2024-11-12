using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

public class VehicleDestroyedPacketProcessor : AuthenticatedPacketProcessor<VehicleDestroyed>
{
    private readonly PlayerManager playerManager;
    private readonly EntitySimulation entitySimulation;
    private readonly WorldEntityManager worldEntityManager;

    public VehicleDestroyedPacketProcessor(PlayerManager playerManager, EntitySimulation entitySimulation, WorldEntityManager worldEntityManager)
    {
        this.playerManager = playerManager;
        this.worldEntityManager = worldEntityManager;
        this.entitySimulation = entitySimulation;
    }

    public override void Process(VehicleDestroyed packet, Player destroyingPlayer)
    {
        entitySimulation.EntityDestroyed(packet.Id);

        if (worldEntityManager.TryDestroyEntity(packet.Id, out Entity entity))
        {
            if (entity is VehicleWorldEntity vehicleWorldEntity)
            {
                worldEntityManager.MovePlayerChildrenToRoot(vehicleWorldEntity);
            }
            foreach (Player player in playerManager.GetConnectedPlayers())
            {
                bool isOtherPlayer = player != destroyingPlayer;
                if (isOtherPlayer && player.CanSee(entity))
                {
                    player.SendPacket(packet);
                }
            }
        }
    }
}

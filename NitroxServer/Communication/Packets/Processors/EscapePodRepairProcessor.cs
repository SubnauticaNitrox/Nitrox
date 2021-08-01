using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.Serialization.World;

namespace NitroxServer.Communication.Packets.Processors
{
    public class EscapePodRepairProcessor : AuthenticatedPacketProcessor<EscapePodRepair>
    {
        private readonly World world;
        private readonly PlayerManager playerManager;

        public EscapePodRepairProcessor(World world, PlayerManager playerManager)
        {
            this.world = world;
            this.playerManager = playerManager;
        }

        public override void Process(EscapePodRepair packet, Player player)
        {
            world.EscapePodManager.RepairEscapePod(packet.Id);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }

}

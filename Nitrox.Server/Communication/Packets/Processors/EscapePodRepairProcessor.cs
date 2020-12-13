using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;
using Nitrox.Server.Serialization.World;

namespace Nitrox.Server.Communication.Packets.Processors
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

using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;
using Nitrox.Server.Serialization.World;

namespace Nitrox.Server.Communication.Packets.Processors
{
    public class EscapePodRadioRepairProcessor : AuthenticatedPacketProcessor<EscapePodRadioRepair>
    {
        private readonly World world;
        private readonly PlayerManager playerManager;

        public EscapePodRadioRepairProcessor(World world, PlayerManager playerManager)
        {
            this.world = world;
            this.playerManager = playerManager;
        }

        public override void Process(EscapePodRadioRepair packet, Player player)
        {
            world.EscapePodManager.RepairEscapePodRadio(packet.Id);
            playerManager.SendPacketToOtherPlayers(packet, player);            
        }
    }

}

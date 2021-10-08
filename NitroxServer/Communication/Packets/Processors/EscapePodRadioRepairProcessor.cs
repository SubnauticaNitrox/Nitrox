using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.Serialization.World;

namespace NitroxServer.Communication.Packets.Processors
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

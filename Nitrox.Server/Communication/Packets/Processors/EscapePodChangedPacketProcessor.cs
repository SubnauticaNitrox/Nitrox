using Nitrox.Model.Logger;
using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;

namespace Nitrox.Server.Communication.Packets.Processors
{
    public class EscapePodChangedPacketProcessor : AuthenticatedPacketProcessor<EscapePodChanged>
    {
        private readonly PlayerManager playerManager;

        public EscapePodChangedPacketProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(EscapePodChanged packet, Player player)
        {
            Log.Debug(packet);
            player.SubRootId = packet.EscapePodId;
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

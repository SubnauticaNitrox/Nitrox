using NitroxModel.Logger;
using NitroxModel_Subnautica.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer_Subnautica.Communication.Packets.Processors
{
    public class RocketEndLaunchProcessor : AuthenticatedPacketProcessor<RocketEndLaunch>
    {
        private readonly PlayerManager playerManager;

        public RocketEndLaunchProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(RocketEndLaunch packet, NitroxServer.Player player)
        {
            Log.Warn($"Starting end sequence with rocket {packet.Id}");
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

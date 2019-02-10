using NitroxModel_Subnautica.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    class CyclopsFireCreatedProcessor : AuthenticatedPacketProcessor<CyclopsFireCreated>
    {
        private readonly PlayerManager playerManager;

        public CyclopsFireCreatedProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(CyclopsFireCreated packet, Player simulatingPlayer)
        {
            playerManager.SendPacketToOtherPlayers(packet, simulatingPlayer);
        }
    }
}

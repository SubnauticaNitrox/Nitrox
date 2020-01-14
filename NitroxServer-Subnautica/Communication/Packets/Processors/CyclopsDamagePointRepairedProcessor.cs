using NitroxModel_Subnautica.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer_Subnautica.Communication.Packets.Processors
{
    class CyclopsDamagePointRepairedProcessor : AuthenticatedPacketProcessor<CyclopsDamagePointRepaired>
    {
        private readonly PlayerManager playerManager;

        public CyclopsDamagePointRepairedProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(CyclopsDamagePointRepaired packet, NitroxServer.Player simulatingPlayer)
        {
            playerManager.SendPacketToOtherPlayers(packet, simulatingPlayer);
        }
    }
}

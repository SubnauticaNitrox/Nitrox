using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    /// <summary>
    /// Process who should win the race towards getting their damage point array validated as the winning one, and send the winner to all
    /// of the losers.
    /// </summary>
    class CyclopsFireHealthChangedProcessor : AuthenticatedPacketProcessor<CyclopsFireHealthChanged>
    {
        private readonly PlayerManager playerManager;

        public CyclopsFireHealthChangedProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(CyclopsFireHealthChanged packet, Player simulatingPlayer)
        {
            playerManager.SendPacketToOtherPlayers(packet, simulatingPlayer);
        }
    }
}

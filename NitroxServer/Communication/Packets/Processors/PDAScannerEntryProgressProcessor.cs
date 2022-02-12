using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Unlockables;

namespace NitroxServer.Communication.Packets.Processors
{
    public class PDAScannerEntryProgressProcessor : AuthenticatedPacketProcessor<PDAEntryProgress>
    {
        private readonly PlayerManager playerManager;
        private readonly PDAStateData pdaStateData;

        public PDAScannerEntryProgressProcessor(PlayerManager playerManager, PDAStateData pdaStateData)
        {
            this.playerManager = playerManager;
            this.pdaStateData = pdaStateData;
        }

        public override void Process(PDAEntryProgress packet, Player player)
        {
            pdaStateData.EntryProgressChanged(packet.TechType, packet.Progress, packet.Unlocked, packet.NitroxId);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

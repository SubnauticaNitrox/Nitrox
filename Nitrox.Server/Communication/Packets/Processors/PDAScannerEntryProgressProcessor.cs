using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;
using Nitrox.Server.GameLogic.Unlockables;

namespace Nitrox.Server.Communication.Packets.Processors
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
            pdaStateData.EntryProgressChanged(packet.TechType, packet.Progress, packet.Unlocked);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

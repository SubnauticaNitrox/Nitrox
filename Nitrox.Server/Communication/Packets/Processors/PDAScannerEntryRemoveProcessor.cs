using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;
using Nitrox.Server.GameLogic.Unlockables;

namespace Nitrox.Server.Communication.Packets.Processors
{
    public class PDAScannerEntryRemoveProcessor : AuthenticatedPacketProcessor<PDAEntryRemove>
    {
        private readonly PlayerManager playerManager;
        private readonly PDAStateData pdaStateData;

        public PDAScannerEntryRemoveProcessor(PlayerManager playerManager, PDAStateData pdaStateData)
        {
            this.playerManager = playerManager;
            this.pdaStateData = pdaStateData;
        }

        public override void Process(PDAEntryRemove packet, Player player)
        {
            pdaStateData.UnlockedTechType(packet.TechType);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

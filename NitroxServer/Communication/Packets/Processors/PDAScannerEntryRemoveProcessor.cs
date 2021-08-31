using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Unlockables;

namespace NitroxServer.Communication.Packets.Processors
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
            if (!pdaStateData.UnlockedTechTypes.Contains(packet.TechType))
            {
                pdaStateData.UnlockedTechType(packet.TechType);
            }
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

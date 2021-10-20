using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Unlockables;

namespace NitroxServer.Communication.Packets.Processors
{
    public class PDAScannerEntryAddProcessor : AuthenticatedPacketProcessor<PDAEntryAdd>
    {
        private readonly PlayerManager playerManager;
        private readonly PDAStateData pdaStateData;

        public PDAScannerEntryAddProcessor(PlayerManager playerManager, PDAStateData pdaStateData)
        {
            this.playerManager = playerManager;
            this.pdaStateData = pdaStateData;
        }

        public override void Process(PDAEntryAdd packet, Player player)
        {
            pdaStateData.EntryProgressChanged(packet.TechType, packet.Progress, packet.Unlocked, null);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

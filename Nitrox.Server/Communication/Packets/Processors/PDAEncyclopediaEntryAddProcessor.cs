using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;
using Nitrox.Server.GameLogic.Unlockables;

namespace Nitrox.Server.Communication.Packets.Processors
{
    public class PDAEncyclopediaEntryAddProcessor : AuthenticatedPacketProcessor<PDAEncyclopediaEntryAdd>
    {
        private readonly PlayerManager playerManager;
        private readonly PDAStateData pdaStateData;

        public PDAEncyclopediaEntryAddProcessor(PlayerManager playerManager, PDAStateData pdaStateData)
        {
            this.playerManager = playerManager;
            this.pdaStateData = pdaStateData;
        }

        public override void Process(PDAEncyclopediaEntryAdd packet, Player player)
        {
            pdaStateData.AddEncyclopediaEntry(packet.Key);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

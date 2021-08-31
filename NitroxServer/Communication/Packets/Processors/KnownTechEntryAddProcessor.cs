using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Unlockables;

namespace NitroxServer.Communication.Packets.Processors
{
    public class KnownTechEntryAddProcessor : AuthenticatedPacketProcessor<KnownTechEntryAdd>
    {
        private readonly PlayerManager playerManager;
        private readonly PDAStateData pdaStateData;

        public KnownTechEntryAddProcessor(PlayerManager playerManager, PDAStateData pdaStateData)
        {
            this.playerManager = playerManager;
            this.pdaStateData = pdaStateData;
        }

        public override void Process(KnownTechEntryAdd packet, Player player)
        {
            if (!pdaStateData.KnownTechTypes.Contains(packet.TechType))
            {
                pdaStateData.AddKnownTechType(packet.TechType);
            }
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }

}

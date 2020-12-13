using Nitrox.Model.Logger;
using Nitrox.Model.Subnautica.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;

namespace Nitrox.Server.Subnautica.Communication.Packets.Processors
{
    /// <summary>
    /// This is the absolute damage state. The current simulation owner is the only one who sends this packet to the server
    /// </summary>
    public class CyclopsDamageProcessor : AuthenticatedPacketProcessor<CyclopsDamage>
    {
        private readonly PlayerManager playerManager;

        public CyclopsDamageProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(CyclopsDamage packet, Player simulatingPlayer)
        {
            Log.Debug("New cyclops damage from " + simulatingPlayer.Id + " " + packet);

            playerManager.SendPacketToOtherPlayers(packet, simulatingPlayer);
        }
    }
}

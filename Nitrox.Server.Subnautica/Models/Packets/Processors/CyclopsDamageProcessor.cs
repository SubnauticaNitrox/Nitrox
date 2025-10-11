using Nitrox.Model.Subnautica.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
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
            Log.Debug($"New cyclops damage from {simulatingPlayer.Id} {packet}");

            playerManager.SendPacketToOtherPlayers(packet, simulatingPlayer);
        }
    }
}

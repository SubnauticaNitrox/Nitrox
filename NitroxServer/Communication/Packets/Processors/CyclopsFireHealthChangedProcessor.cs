using System.Collections.Generic;
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

        /// <summary>
        /// Contains a list of all Cyclops that have been damaged. Undamaged Cyclops are not pre-loaded. It stores the Cyclops guid and damage point indexes.
        /// </summary>
        private static readonly Dictionary<string, CyclopsDamage> cyclopsDamagePoints = new Dictionary<string, CyclopsDamage>();

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

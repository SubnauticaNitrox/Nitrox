﻿using System.Linq;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
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

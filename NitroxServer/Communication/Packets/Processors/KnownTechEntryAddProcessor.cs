using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Items;

namespace NitroxServer.Communication.Packets.Processors
{
    public class KnownTechEntryAddProcessor : AuthenticatedPacketProcessor<KnownTechEntryAdd>
    {
        private readonly PlayerManager playerManager;
        private readonly GameData gameData;

        public KnownTechEntryAddProcessor(PlayerManager playerManager, GameData gameData)
        {
            this.playerManager = playerManager;
            this.gameData = gameData;
        }

        public override void Process(KnownTechEntryAdd packet, Player player)
        {
            Log.Info(packet.ToString());
            gameData.PDAState.PDADataknownTech.Add(packet.TechType);
            playerManager.SendPacketToOtherPlayers(packet, player);
            
        }
    }

}

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
    public class PDALogEntryAddProcessor : AuthenticatedPacketProcessor<PDALogEntryAdd>
    {
        private readonly PlayerManager playerManager;
        private readonly GameData gameData;
        public PDALogEntryAddProcessor(PlayerManager playerManager, GameData gameData)
        {
            this.playerManager = playerManager;
            this.gameData = gameData;
        }

        public override void Process(PDALogEntryAdd packet, Player player)
        {
            gameData.PDAState.AddPDALogEntry(new PDALogEntry { Key = packet.Key, Timestamp = packet.Timestamp });
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

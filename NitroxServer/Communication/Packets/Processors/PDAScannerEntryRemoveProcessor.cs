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
    public class PDAScannerEntryRemoveProcessor : AuthenticatedPacketProcessor<PDAEntryRemove>
    {
        private readonly PlayerManager playerManager;
        private readonly GameData gameData;

        public PDAScannerEntryRemoveProcessor(PlayerManager playerManager, GameData gameData)
        {
            this.gameData = gameData;
            this.playerManager = playerManager;
        }

        public override void Process(PDAEntryRemove packet, Player player)
        {
            gameData.PDAState.PDADataPartial.Delete(new PDAEntry { Progress = packet.Progress, TechType = packet.TechType, Unlocked = packet.Unlocked });
            gameData.PDAState.PDADataComplete.Add(packet.TechType);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

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
    public class PDAScannerEntryProgressProcessor : AuthenticatedPacketProcessor<PDAEntryProgress>
    {
        private readonly PlayerManager playerManager;
        private readonly GameData gameData;
        public PDAScannerEntryProgressProcessor(PlayerManager playerManager, GameData gameData)
        {
            this.playerManager = playerManager;
            this.gameData = gameData;
        }

        public override void Process(PDAEntryProgress packet, Player player)
        {
            PDAEntry entry;
            if (gameData.PDAState.PDADataPartial.Contain(packet.TechType, out entry))
            {
                entry.Unlocked = packet.Unlocked;
            }
            else
            {
                gameData.PDAState.PDADataPartial.Add(new PDAEntry { Progress = packet.Progress, TechType = packet.TechType, Unlocked = packet.Unlocked });
            }
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

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
    public class PDAScannerEntryProcessorAdd : AuthenticatedPacketProcessor<PDAEntryAdd>
    {
        private readonly PlayerManager playerManager;
        private readonly GameData gameData;
        public PDAScannerEntryProcessorAdd(PlayerManager playerManager, GameData gameData)
        {
            this.playerManager = playerManager;
            this.gameData = gameData;
        }

        public override void Process(PDAEntryAdd packet, Player player)
        {
            gameData.PDAState.PDADataPartial.Add(new PDAEntry { Progress= packet.Progress, TechType = packet.TechType, Unlocked = packet.Unlocked });
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }

    public class PDAScannerEntryProcessorProgress : AuthenticatedPacketProcessor<PDAEntryProgress>
    {
        private readonly PlayerManager playerManager;
        private readonly GameData gameData;
        public PDAScannerEntryProcessorProgress(PlayerManager playerManager, GameData gameData)
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

    public class PDAScannerEntryProcessorRemove : AuthenticatedPacketProcessor<PDAEntryRemove>
    {
        private readonly PlayerManager playerManager;
        private readonly GameData gameData;

        public PDAScannerEntryProcessorRemove(PlayerManager playerManager, GameData gameData)
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

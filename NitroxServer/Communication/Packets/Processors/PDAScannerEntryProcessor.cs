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
<<<<<<< HEAD
<<<<<<< HEAD
            gameData.PDAState.PDADataPartial.Add(new PDAEntry { Progress= packet.Progress, TechType = packet.TechType, Unlocked = packet.Unlocked });
=======
            gameData.PDASaveData.PDADataPartial.Add(new PDA_Entry { Progress= packet.Progress, TechType = packet.TechType, Unlocked = packet.Unlocked });
>>>>>>> 08eed5b... Sync And Save (KnownTech Entries,PDAScanner Entries,PDAEncyclopediaEntries )
=======
            gameData.PDAState.PDADataPartial.Add(new PDAEntry { Progress= packet.Progress, TechType = packet.TechType, Unlocked = packet.Unlocked });
>>>>>>> c7606c2... Changes Requested
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
<<<<<<< HEAD
<<<<<<< HEAD
            PDAEntry entry;
            if (gameData.PDAState.PDADataPartial.Contain(packet.TechType, out entry))
=======
           
            PDA_Entry entry;
            if (gameData.PDASaveData.PDADataPartial.Contain(packet.TechType, out entry))
>>>>>>> 08eed5b... Sync And Save (KnownTech Entries,PDAScanner Entries,PDAEncyclopediaEntries )
=======
            PDAEntry entry;
            if (gameData.PDAState.PDADataPartial.Contain(packet.TechType, out entry))
>>>>>>> c7606c2... Changes Requested
            {
                entry.Unlocked = packet.Unlocked;
            }
            else
            {
<<<<<<< HEAD
<<<<<<< HEAD
                gameData.PDAState.PDADataPartial.Add(new PDAEntry { Progress = packet.Progress, TechType = packet.TechType, Unlocked = packet.Unlocked });
=======
                gameData.PDASaveData.PDADataPartial.Add(new PDA_Entry { Progress = packet.Progress, TechType = packet.TechType, Unlocked = packet.Unlocked });
>>>>>>> 08eed5b... Sync And Save (KnownTech Entries,PDAScanner Entries,PDAEncyclopediaEntries )
=======
                gameData.PDAState.PDADataPartial.Add(new PDAEntry { Progress = packet.Progress, TechType = packet.TechType, Unlocked = packet.Unlocked });
>>>>>>> c7606c2... Changes Requested
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
<<<<<<< HEAD
<<<<<<< HEAD
            gameData.PDAState.PDADataPartial.Delete(new PDAEntry { Progress = packet.Progress, TechType = packet.TechType, Unlocked = packet.Unlocked });
            gameData.PDAState.PDADataComplete.Add(packet.TechType);
=======
            gameData.PDASaveData.PDADataPartial.Delete(new PDA_Entry { Progress = packet.Progress, TechType = packet.TechType, Unlocked = packet.Unlocked });
            gameData.PDASaveData.PDADataComplete.Add(packet.TechType);
>>>>>>> 08eed5b... Sync And Save (KnownTech Entries,PDAScanner Entries,PDAEncyclopediaEntries )
=======
            gameData.PDAState.PDADataPartial.Delete(new PDAEntry { Progress = packet.Progress, TechType = packet.TechType, Unlocked = packet.Unlocked });
            gameData.PDAState.PDADataComplete.Add(packet.TechType);
>>>>>>> c7606c2... Changes Requested
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

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
    public class PDAEncyclopediaEntryProcessorAdd : AuthenticatedPacketProcessor<PDAEncyclopediaEntryAdd>
    {
        private readonly PlayerManager playerManager;
        private readonly GameData gameData;

        public PDAEncyclopediaEntryProcessorAdd(PlayerManager playerManager, GameData gameData)
        {
            this.playerManager = playerManager;
            this.gameData = gameData;
        }

        public override void Process(PDAEncyclopediaEntryAdd packet, Player player)
        {
<<<<<<< HEAD
<<<<<<< HEAD
            gameData.PDAState.PDADataEnciclopedia.Add(packet.Key);
=======
            gameData.PDASaveData.PDADataEnciclopedia.Add(packet.Key);
>>>>>>> 08eed5b... Sync And Save (KnownTech Entries,PDAScanner Entries,PDAEncyclopediaEntries )
=======
            gameData.PDAState.PDADataEnciclopedia.Add(packet.Key);
>>>>>>> c7606c2... Changes Requested
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }

    public class PDAEncyclopediaEntryProcessorUpdate : AuthenticatedPacketProcessor<PDAEncyclopediaEntryUpdate>
    {
        private readonly PlayerManager playerManager;

        public PDAEncyclopediaEntryProcessorUpdate(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(PDAEncyclopediaEntryUpdate packet, Player player)
        {
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }

    public class PDAEncyclopediaEntryProcessorRemove : AuthenticatedPacketProcessor<PDAEncyclopediaEntryRemove>
    {
        private readonly PlayerManager playerManager;

        public PDAEncyclopediaEntryProcessorRemove(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(PDAEncyclopediaEntryRemove packet, Player player)
        {
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

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
    public class KnownTechEntryProcessorAdd : AuthenticatedPacketProcessor<KnownTechEntryAdd>
    {
        private readonly PlayerManager playerManager;
        private readonly GameData gameData;

        public KnownTechEntryProcessorAdd(PlayerManager playerManager, GameData gameData)
        {
            this.playerManager = playerManager;
            this.gameData = gameData;
        }

        public override void Process(KnownTechEntryAdd packet, Player player)
        {
            Log.Info(packet.ToString());
            gameData.PDASaveData.PDADataknownTech.Add(packet.TechType);
            playerManager.SendPacketToOtherPlayers(packet, player);
            
        }
    }

    public class KnownTechEntryProcessorUpdate : AuthenticatedPacketProcessor<KnownTechEntryChanged>
    {
        private readonly PlayerManager playerManager;

        public KnownTechEntryProcessorUpdate(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(KnownTechEntryChanged packet, Player player)
        {
            playerManager.SendPacketToOtherPlayers(packet, player);
            Log.Info(packet.ToString());
        }
    }

    public class KnownTechEntryProcessorRemove : AuthenticatedPacketProcessor<KnownTechEntryRemove>
    {
        private readonly PlayerManager playerManager;

        public KnownTechEntryProcessorRemove(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(KnownTechEntryRemove packet, Player player)
        {
            playerManager.SendPacketToOtherPlayers(packet, player);
            Log.Info(packet.ToString());
        }
    }
}

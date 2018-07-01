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
    public class SignPacketProcessor : AuthenticatedPacketProcessor<SignChanged>
    {
        private readonly PlayerManager playerManager;
        private readonly BaseSign signData;

        public SignPacketProcessor(PlayerManager playerManager, BaseSign signData)
        {
            this.playerManager = playerManager;
            this.signData = signData;
        }

        public override void Process(SignChanged packet, Player player)
        {
            signData.UpdateBaseSign(new SignData(packet.Guid, packet.NewText, packet.ColorIndex, packet.ScaleIndex, packet.Elements, packet.Background));
            playerManager.SendPacketToOtherPlayers(packet, player);
            Log.Info(packet.ToString());
        }
    }
}

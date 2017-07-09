using NitroxClient.Communication.Packets.Processors.Base;
using NitroxClient.GameLogic;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxClient.Communication.Packets.Processors
{
    class DisconnectProcessor : GenericPacketProcessor<Disconnect>
    {
        private PlayerGameObjectManager playerGameObjectManager;

        public DisconnectProcessor(PlayerGameObjectManager playerGameObjectManager)
        {
            this.playerGameObjectManager = playerGameObjectManager;
        }

        public override void Process(Disconnect disconnect)
        {
            playerGameObjectManager.RemovePlayerGameObject(disconnect.PlayerId);
        }
    }
}

using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

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

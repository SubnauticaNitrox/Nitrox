using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.Logger;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    class DisconnectProcessor : ClientPacketProcessor<Disconnect>
    {
        private PlayerManager remotePlayerManager;

        public DisconnectProcessor(PlayerManager remotePlayerManager)
        {
            this.remotePlayerManager = remotePlayerManager;
        }

        public override void Process(Disconnect disconnect)
        {
            remotePlayerManager.RemovePlayer(disconnect.PlayerId);
            ClientLogger.WriteLine(disconnect.PlayerId + " disconnected");
        }
    }
}

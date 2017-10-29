using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.HUD;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    class DisconnectProcessor : ClientPacketProcessor<Disconnect>
    {
        private readonly PlayerManager remotePlayerManager;
        private readonly PlayerVitalsManager vitalsManager;

        public DisconnectProcessor(PlayerManager remotePlayerManager, PlayerVitalsManager vitalsManager)
        {
            this.remotePlayerManager = remotePlayerManager;
            this.vitalsManager = vitalsManager;
        }

        public override void Process(Disconnect disconnect)
        {
            //TODO: don't remove right away... maybe grey out and start 
            //      a coroutine to finally remove.
            vitalsManager.RemovePlayer(disconnect.PlayerId);

            remotePlayerManager.RemovePlayer(disconnect.PlayerId);

            Log.InGame(disconnect.PlayerId + " disconnected");
        }
    }
}

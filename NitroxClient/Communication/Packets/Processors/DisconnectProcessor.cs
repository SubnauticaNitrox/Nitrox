using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.HUD;
using NitroxModel.DataStructures.Util;
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
            // TODO: don't remove right away... maybe grey out and start
            //      a coroutine to finally remove.
            vitalsManager.RemoveForPlayer(disconnect.PlayerId);

            Optional<RemotePlayer> remotePlayer = remotePlayerManager.Find(disconnect.PlayerId);
            if (remotePlayer.HasValue)
            {
                remotePlayerManager.RemovePlayer(disconnect.PlayerId);
                Log.InGame(remotePlayer.Value.PlayerName + " disconnected");
            }
        }
    }
}

using NitroxClient.GameLogic;
using NitroxClient.GameLogic.HUD;
using NitroxModel.DataStructures.Util;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    class DisconnectProcessor : IClientPacketProcessor<Disconnect>
    {
        private readonly PlayerManager remotePlayerManager;
        private readonly PlayerVitalsManager vitalsManager;

        public DisconnectProcessor(PlayerManager remotePlayerManager, PlayerVitalsManager vitalsManager)
        {
            this.remotePlayerManager = remotePlayerManager;
            this.vitalsManager = vitalsManager;
        }

        public Task Process(IPacketProcessContext context, Disconnect disconnect)
        {
            // TODO: don't remove right away... maybe grey out and start
            //      a coroutine to finally remove.
            vitalsManager.RemoveForPlayer(disconnect.SessionId);

            Optional<RemotePlayer> remotePlayer = remotePlayerManager.Find(disconnect.SessionId);
            if (remotePlayer.HasValue)
            {
                remotePlayer.Value.PlayerDisconnectEvent.Trigger(remotePlayer.Value);
                remotePlayerManager.RemovePlayer(disconnect.SessionId);
                Log.Info($"{remotePlayer.Value.PlayerName} disconnected");
                Log.InGame(Language.main.Get("Nitrox_PlayerDisconnected").Replace("{PLAYER}", remotePlayer.Value.PlayerName));
            }
            return Task.CompletedTask;
        }
    }
}

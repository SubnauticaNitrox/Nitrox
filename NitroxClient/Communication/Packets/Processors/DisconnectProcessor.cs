using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.HUD;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class DisconnectProcessor(PlayerManager remotePlayerManager, PlayerVitalsManager vitalsManager) : IClientPacketProcessor<Disconnect>
{
    private readonly PlayerManager remotePlayerManager = remotePlayerManager;
    private readonly PlayerVitalsManager vitalsManager = vitalsManager;

    public Task Process(ClientProcessorContext context, Disconnect disconnect)
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

using Nitrox.Model.Helper;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class PlayerDeathProcessor(PlayerManager playerManager) : IClientPacketProcessor<PlayerDeathEvent>
{
    private readonly PlayerManager playerManager = playerManager;

    public Task Process(ClientProcessorContext context, PlayerDeathEvent playerDeath)
    {
        RemotePlayer player = Validate.IsPresent(playerManager.Find(playerDeath.SessionId));
        Log.Debug($"{player.PlayerName} died");
        Log.InGame(Language.main.Get("Nitrox_PlayerDied").Replace("{PLAYER}", player.PlayerName));
        player.PlayerDeathEvent.Trigger(player);
        return Task.CompletedTask;

        // TODO: Add any death related triggers (i.e. scoreboard updates, rewards, etc.)
    }
}

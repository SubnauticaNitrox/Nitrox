using NitroxClient.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class PlayerDeathProcessor : IClientPacketProcessor<PlayerDeathEvent>
{
    private readonly PlayerManager playerManager;

    public PlayerDeathProcessor(PlayerManager playerManager)
    {
        this.playerManager = playerManager;
    }

    public Task Process(IPacketProcessContext context, PlayerDeathEvent playerDeath)
    {
        RemotePlayer player = Validate.IsPresent(playerManager.Find(playerDeath.PlayerId));
        Log.Debug($"{player.PlayerName} died");
        Log.InGame(Language.main.Get("Nitrox_PlayerDied").Replace("{PLAYER}", player.PlayerName));
        player.PlayerDeathEvent.Trigger(player);

        // TODO: Add any death related triggers (i.e. scoreboard updates, rewards, etc.)

        return Task.CompletedTask;
    }
}

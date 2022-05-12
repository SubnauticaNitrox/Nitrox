using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class PlayerDeathProcessor : ClientPacketProcessor<PlayerDeathEvent>
{
    private readonly PlayerManager playerManager;

    public PlayerDeathProcessor(PlayerManager playerManager)
    {
        this.playerManager = playerManager;
    }

    public override void Process(PlayerDeathEvent playerDeath)
    {
        RemotePlayer player = Validate.IsPresent(playerManager.Find(playerDeath.PlayerId));
        Log.InGame($"{player.PlayerName} died");
        player.PlayerDeathEvent.Trigger(player);
        
        // TODO: Add any death related triggers (i.e. scoreboard updates, rewards, etc.)
    }
}

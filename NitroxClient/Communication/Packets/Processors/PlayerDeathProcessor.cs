using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    class PlayerDeathProcessor : ClientPacketProcessor<PlayerDeathEvent>
    {
        public override void Process(PlayerDeathEvent playerDeath)
        {
            // TODO: Add any death related triggers (i.e. scoreboard updates, rewards, etc.)
            Log.InGame(playerDeath.PlayerName + " died");
        }
    }
}

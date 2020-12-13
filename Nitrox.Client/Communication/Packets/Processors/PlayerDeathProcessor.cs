using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Model.Logger;
using Nitrox.Model.Packets;

namespace Nitrox.Client.Communication.Packets.Processors
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

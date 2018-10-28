using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    class PlayerDeathProcessor : ClientPacketProcessor<PlayerDeathEvent>
    {
        private readonly INitroxLogger log;

        public PlayerDeathProcessor(INitroxLogger logger)
        {
            log = logger;
        }

        public override void Process(PlayerDeathEvent playerDeath)
        {
            // TODO: Add any death related triggers (i.e. scoreboard updates, rewards, etc.)
            log.InGame($"{playerDeath.PlayerId} died");
        }
    }
}

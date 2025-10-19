using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    public class ScheduleProcessor : AuthenticatedPacketProcessor<Schedule>
    {
        private readonly PlayerManager playerManager;
        private readonly ScheduleKeeper scheduleKeeper;

        public ScheduleProcessor(PlayerManager playerManager, ScheduleKeeper scheduleKeeper)
        {
            this.playerManager = playerManager;
            this.scheduleKeeper = scheduleKeeper;
        }

        public override void Process(Schedule packet, Player player)
        {
            scheduleKeeper.ScheduleGoal(NitroxScheduledGoal.From(packet.TimeExecute, packet.Key, packet.Type));
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

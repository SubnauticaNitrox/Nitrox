using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
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

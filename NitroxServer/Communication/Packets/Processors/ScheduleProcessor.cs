using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    class ScheduleProcessor : AuthenticatedPacketProcessor<Schedule>
    {
        private readonly ScheduleKeeper scheduleKeeper;

        public ScheduleProcessor(ScheduleKeeper scheduleKeeper)
        {
            this.scheduleKeeper = scheduleKeeper;
        }

        public override void Process(Schedule packet, Player simulatingPlayer)
        {
            scheduleKeeper.ScheduleGoal(FakeScheduledGoal.From(packet.TimeExecute, packet.Key, packet.Type));
        }
    }
}

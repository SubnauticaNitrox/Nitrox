using NitroxModel.Core;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    public class BedEnterProcessor : AuthenticatedPacketProcessor<BedEnter>
    {
        private readonly ScheduleKeeper scheduleKeeper;

        public BedEnterProcessor(ScheduleKeeper scheduleKeeper)
        {
            this.scheduleKeeper = scheduleKeeper;
        }

        public override void Process(BedEnter packet, Player player)
        {
            scheduleKeeper.ChangeTime(ScheduleKeeper.TimeModification.SKIP);
        }
    }
}

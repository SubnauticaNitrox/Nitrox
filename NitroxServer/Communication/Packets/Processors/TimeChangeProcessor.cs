using System;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    /// <summary>
    /// The client can ask for an update of the server's current time. If it passes the current Utc time in milliseconds, it can track
    /// the latency and make the server time passed much more accurate.
    /// </summary>
    public class TimeChangeProcessor : AuthenticatedPacketProcessor<TimeChange>
    {
        private readonly TimeKeeper timeKeeper;

        public TimeChangeProcessor(TimeKeeper timeKeeper)
        {
            this.timeKeeper = timeKeeper;
        }

        public override void Process(TimeChange timeChangePacket, Player player)
        {
            double totalMilliseconds = new TimeSpan(DateTime.UtcNow.Ticks).TotalMilliseconds;
            player.SendPacket(new TimeChange(timeKeeper.GetCurrentTime(), totalMilliseconds, timeChangePacket.ClientSentAtMilliseconds));
        }
    }
}

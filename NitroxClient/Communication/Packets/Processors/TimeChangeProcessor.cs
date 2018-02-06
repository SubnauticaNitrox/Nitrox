using System;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class TimeChangeProcessor : ClientPacketProcessor<TimeChange>
    {
        private readonly IPacketSender packetSender;

        public TimeChangeProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(TimeChange timeChangePacket)
        {
            // If there's no delay, we need to reply with another request with the delay to calculate latency. Not the most accurate,
            // but it'll work for synchronizing random number generators. Also allows us to do this multiple times if we want to make
            // the time more accurate.
            if (timeChangePacket.ClientSentAtMilliseconds == 0)
            {
                double totalMilliseconds = new TimeSpan(DateTime.UtcNow.Ticks).TotalMilliseconds;
                packetSender.Send(new TimeChange(timeChangePacket.CurrentGameTime, timeChangePacket.CurrentServerUtcTimeMilliseconds, totalMilliseconds));
            }
            else
            {
                Multiplayer.Logic.ServerTime.SetServerUtcTime(timeChangePacket.CurrentServerUtcTimeMilliseconds,
                    timeChangePacket.ClientSentAtMilliseconds, 
                    DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond);

                // The Day/night cycle looks to only be accurate down to the seconds. 
                DayNightCycle.main.timePassedAsDouble = timeChangePacket.CurrentGameTime; //TODO: account for player latency
            }
        }
    }
}

using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Model.Packets;

namespace Nitrox.Client.Communication.Packets.Processors
{
    public class TimeChangeProcessor : ClientPacketProcessor<TimeChange>
    {
        public override void Process(TimeChange timeChangePacket)
        {
            DayNightCycle.main.timePassedAsDouble = timeChangePacket.CurrentTime; //TODO: account for player latency
        }
    }
}

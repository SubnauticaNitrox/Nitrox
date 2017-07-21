using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class TimeChangeProcessor : GenericPacketProcessor<TimeChange>
    {
        public override void Process(TimeChange timeChangePacket)
        {
            DayNightCycle.main.timePassed = timeChangePacket.CurrentTime; //TODO: account for player latency
        }
    }
}

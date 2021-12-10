using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    public class BedEnterProcessor : AuthenticatedPacketProcessor<BedEnter>
    {
        private readonly EventTriggerer eventTriggerer;

        public BedEnterProcessor(EventTriggerer eventTriggerer)
        {
            this.eventTriggerer = eventTriggerer;
        }

        public override void Process(BedEnter packet, Player player)
        {
            eventTriggerer.ChangeTime(EventTriggerer.TimeModification.SKIP);
        }
    }
}

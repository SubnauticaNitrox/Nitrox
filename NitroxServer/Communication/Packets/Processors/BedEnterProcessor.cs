using NitroxModel.Core;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    public class BedEnterProcessor : AuthenticatedPacketProcessor<BedEnter>
    {
        public override void Process(BedEnter packet, Player player)
        {
            TimeKeeper timeKeeper = NitroxServiceLocator.LocateService<TimeKeeper>();
            timeKeeper.SkipTime();
        }
    }
}

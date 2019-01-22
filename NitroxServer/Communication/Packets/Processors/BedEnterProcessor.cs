using NitroxModel.Core;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.ConsoleCommands.Processor;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Players;

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

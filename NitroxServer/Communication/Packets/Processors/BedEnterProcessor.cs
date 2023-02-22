using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    public class BedEnterProcessor : AuthenticatedPacketProcessor<BedEnter>
    {
        private readonly StoryManager storyManager;

        public BedEnterProcessor(StoryManager storyManager)
        {
            this.storyManager = storyManager;
        }

        public override void Process(BedEnter packet, Player player)
        {
            // TODO: Needs repair since the new time implementation only relies on server-side time.
            // storyManager.ChangeTime(StoryManager.TimeModification.SKIP);
        }
    }
}

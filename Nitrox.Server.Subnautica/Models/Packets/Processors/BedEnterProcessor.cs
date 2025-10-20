using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    internal sealed class BedEnterProcessor : AuthenticatedPacketProcessor<BedEnter>
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

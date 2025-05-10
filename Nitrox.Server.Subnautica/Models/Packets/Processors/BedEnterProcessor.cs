using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class BedEnterProcessor : IAuthPacketProcessor<BedEnter>
{
    public async Task Process(AuthProcessorContext context, BedEnter packet)
    {
        // TODO: Needs repair since the new time implementation only relies on server-side time.
        // storyTimingService.ChangeTime(StoryTimingService.TimeModification.SKIP);
    }
}

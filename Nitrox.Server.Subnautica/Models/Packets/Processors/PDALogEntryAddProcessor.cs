using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PDALogEntryAddProcessor(IPacketSender packetSender, PdaManager pdaManager, StoryScheduler storyScheduler) : AuthenticatedPacketProcessor<PDALogEntryAdd>
{
    private readonly IPacketSender packetSender = packetSender;
    private readonly PdaManager pdaManager = pdaManager;
    private readonly StoryScheduler storyScheduler = storyScheduler;

    public override void Process(PDALogEntryAdd packet, Player player)
    {
        pdaManager.AddPDALogEntry(new PDALogEntry(packet.Key, packet.Timestamp));
        if (storyScheduler.ContainsScheduledStory(packet.Key))
        {
            storyScheduler.UnscheduleStory(packet.Key);
        }
        packetSender.SendPacketToOthersAsync(packet, player.SessionId);
    }
}

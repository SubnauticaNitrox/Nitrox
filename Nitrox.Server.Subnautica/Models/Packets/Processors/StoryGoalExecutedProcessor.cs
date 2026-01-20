using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class StoryGoalExecutedProcessor(IPacketSender packetSender, StoryManager storyManager, StoryScheduler storyScheduler, PdaManager pdaManager, ILogger<StoryGoalExecutedProcessor> logger)
    : AuthenticatedPacketProcessor<StoryGoalExecuted>
{
    private readonly IPacketSender packetSender = packetSender;
    private readonly StoryManager storyManager = storyManager;
    private readonly StoryScheduler storyScheduler = storyScheduler;
    private readonly PdaManager pdaManager = pdaManager;
    private readonly ILogger<StoryGoalExecutedProcessor> logger = logger;

    public override void Process(StoryGoalExecuted packet, Player player)
    {
        logger.ZLogDebug($"Processing packet: {packet}");
        // The switch is structure is similar to StoryGoal.Execute()
        bool added = storyManager.AddCompletedStory(packet.Key);
        switch (packet.Type)
        {
            case StoryGoalExecuted.EventType.RADIO:
                if (added)
                {
                    storyManager.QueueRadioStory(packet.Key);
                }
                break;
            case StoryGoalExecuted.EventType.PDA:
                if (packet.Timestamp.HasValue)
                {
                    pdaManager.AddPDALogEntry(new(packet.Key, packet.Timestamp.Value));
                }
                break;
        }
        storyScheduler.UnscheduleStory(packet.Key);
        packetSender.SendPacketToOthersAsync(packet, player.SessionId);
    }
}

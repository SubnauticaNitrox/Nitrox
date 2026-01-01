using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class StoryGoalExecutedProcessor : AuthenticatedPacketProcessor<StoryGoalExecuted>
{
    private readonly PlayerManager playerManager;
    private readonly StoryManager storyManager;
    private readonly StoryScheduler storyScheduler;
    private readonly PdaManager pdaManager;
    private readonly ILogger<StoryGoalExecutedProcessor> logger;

    public StoryGoalExecutedProcessor(PlayerManager playerManager,  StoryManager storyManager, StoryScheduler storyScheduler, PdaManager pdaManager, ILogger<StoryGoalExecutedProcessor> logger)
    {
        this.playerManager = playerManager;
        this.storyManager = storyManager;
        this.storyScheduler = storyScheduler;
        this.pdaManager = pdaManager;
        this.logger = logger;
    }

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

        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}

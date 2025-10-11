using Nitrox.Model.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Unlockables;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class StoryGoalExecutedProcessor : AuthenticatedPacketProcessor<StoryGoalExecuted>
{
    private readonly PlayerManager playerManager;
    private readonly StoryGoalData storyGoalData;
    private readonly ScheduleKeeper scheduleKeeper;
    private readonly PDAStateData pdaStateData;

    public StoryGoalExecutedProcessor(PlayerManager playerManager,  StoryGoalData storyGoalData, ScheduleKeeper scheduleKeeper, PDAStateData pdaStateData)
    {
        this.playerManager = playerManager;
        this.storyGoalData = storyGoalData;
        this.scheduleKeeper = scheduleKeeper;
        this.pdaStateData = pdaStateData;
    }

    public override void Process(StoryGoalExecuted packet, Player player)
    {
        Log.Debug($"Processing StoryGoalExecuted: {packet}");
        // The switch is structure is similar to StoryGoal.Execute()
        bool added = storyGoalData.CompletedGoals.Add(packet.Key);
        switch (packet.Type)
        {
            case StoryGoalExecuted.EventType.RADIO:
                if (added)
                {
                    storyGoalData.RadioQueue.Enqueue(packet.Key);
                }
                break;
            case StoryGoalExecuted.EventType.PDA:
                if (packet.Timestamp.HasValue)
                {
                    pdaStateData.AddPDALogEntry(new(packet.Key, packet.Timestamp.Value));
                }
                break;
        }

        scheduleKeeper.UnScheduleGoal(packet.Key);

        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}

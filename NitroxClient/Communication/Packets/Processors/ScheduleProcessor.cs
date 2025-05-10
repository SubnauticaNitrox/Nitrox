using System.Linq;
using NitroxModel.Networking.Packets;
using Story;

namespace NitroxClient.Communication.Packets.Processors;

public class ScheduleProcessor : IClientPacketProcessor<Schedule>
{
    public Task Process(IPacketProcessContext context, Schedule schedulePacket)
    {
        ScheduledGoal goal = new()
        {
            goalKey = schedulePacket.Key,
            goalType = (Story.GoalType)schedulePacket.Category,
            timeExecute = schedulePacket.TimeExecute
        };
        if (ShouldSchedule(goal))
        {
            StoryGoalScheduler.main.schedule.Add(goal);
        }
        Log.Debug($"Processed a Schedule packet [Key: {goal.goalKey}, Type: {goal.goalType}, TimeExecute: {goal.timeExecute}]");

        return Task.CompletedTask;
    }

    private bool ShouldSchedule(ScheduledGoal goal)
    {
        return goal.timeExecute >= DayNightCycle.main.timePassedAsDouble && !IsAlreadyKnown(goal.goalKey);
    }

    private bool IsAlreadyKnown(string goalKey)
    {
        return StoryGoalScheduler.main.schedule.Any(g => g.goalKey == goalKey) || //  Scheduled
               StoryGoalManager.main.completedGoals.Contains(goalKey); // Completed
    }
}

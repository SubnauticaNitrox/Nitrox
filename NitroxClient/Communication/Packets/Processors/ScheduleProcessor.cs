﻿using System.Linq;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;
using Story;

namespace NitroxClient.Communication.Packets.Processors
{
    public class ScheduleProcessor : ClientPacketProcessor<Schedule>
    {
        public override void Process(Schedule schedulePacket)
        {
            ScheduledGoal goal = new()
            {
                goalKey = schedulePacket.Key,
                goalType = (Story.GoalType)System.Enum.Parse(typeof(Story.GoalType), schedulePacket.Type),
                timeExecute = schedulePacket.TimeExecute
            };
            if (ShouldSchedule(goal.timeExecute) && !IsAlreadyKnown(goal.goalKey))
            {
                StoryGoalScheduler.main.schedule.Add(goal);
            }
            Log.Debug($"Processed a Schedule packet [Key: {goal.goalKey}, Type: {goal.goalType}, TimeExecute: {goal.timeExecute}]");
        }

        private bool ShouldSchedule(float timeToExecute)
        {
            return timeToExecute >= DayNightCycle.main.timePassedAsDouble;
        }
        private bool IsAlreadyKnown(string goalKey)
        {
            return StoryGoalScheduler.main.schedule.Any(g => g.goalKey == goalKey) //  Scheduled
                   || PDALog.entries.ContainsKey(goalKey) //  Registered
                   || StoryGoalManager.main.completedGoals.Contains(goalKey); // Completed
        }
    }
}

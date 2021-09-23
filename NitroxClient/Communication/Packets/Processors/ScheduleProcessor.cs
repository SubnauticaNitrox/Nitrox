using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Logger;
using NitroxModel.Packets;
using Story;

namespace NitroxClient.Communication.Packets.Processors
{
    public class ScheduleProcessor : ClientPacketProcessor<Schedule>
    {
        private readonly FieldInfo entriesField = typeof(PDALog).GetField("entries", BindingFlags.NonPublic | BindingFlags.Static);
        public override void Process(Schedule schedulePacket)
        {
            ScheduledGoal goal = new()
            {
                version = 1,
                goalKey = schedulePacket.Key,
                goalType = (Story.GoalType)Enum.Parse(typeof(Story.GoalType), schedulePacket.Type),
                timeExecute = schedulePacket.TimeExecute
            };
            if (ShouldSchedule(goal.timeExecute) && !IsAlreadyKnown(goal.goalKey))
            {
                StoryGoalScheduler.main.schedule.Add(goal);
            }
            Log.Debug($"Processed a Schedule packet [{goal}]");
        }

        private bool ShouldSchedule(float timeToExecute)
        {
            return timeToExecute >= DayNightCycle.main.timePassedAsDouble;
        }
        private bool IsAlreadyKnown(string goalKey)
        {
            Dictionary<string, PDALog.Entry> entries = (Dictionary<string, PDALog.Entry>)entriesField.GetValue(null);
            return StoryGoalScheduler.main.schedule.Any(g => g.goalKey == goalKey) //  Scheduled
                   || entries.ContainsKey(goalKey) //  Registered
                   || StoryGoalManager.main.completedGoals.Contains(goalKey); // Completed
        }
    }
}

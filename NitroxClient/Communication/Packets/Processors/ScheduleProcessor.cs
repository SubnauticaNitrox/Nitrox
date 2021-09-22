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
        public override void Process(Schedule schedulePacket)
        {
            Dictionary<string, PDALog.Entry> entries = (Dictionary<string, PDALog.Entry>)(typeof(PDALog).GetField("entries", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));
            ScheduledGoal goal = new ScheduledGoal();
            goal.version = 1;
            goal.goalKey = schedulePacket.Key;
            goal.goalType = (Story.GoalType)System.Enum.Parse(typeof(Story.GoalType), schedulePacket.Type);
            goal.timeExecute = schedulePacket.TimeExecute;
            if (goal.timeExecute >= DayNightCycle.main.timePassedAsDouble)
            {
                if (!StoryGoalScheduler.main.schedule.Any(alreadyInGoal => alreadyInGoal.goalKey == goal.goalKey) && !entries.TryGetValue(goal.goalKey, out PDALog.Entry value) && !StoryGoalManager.main.completedGoals.Contains(schedulePacket.Key))
                {
                    StoryGoalScheduler.main.schedule.Add(goal);
                }
            }
            Log.Debug($"Processed a Schedule packet [{goal}]");
        }
    }
}

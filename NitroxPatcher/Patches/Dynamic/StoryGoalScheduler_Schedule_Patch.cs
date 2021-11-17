using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxModel.Helper;
using NitroxModel.Packets;
using Story;

namespace NitroxPatcher.Patches.Dynamic
{
    public class StoryGoalScheduler_Schedule_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(StoryGoalScheduler);
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((StoryGoalScheduler t) => t.Schedule(default(StoryGoal)));
        private static readonly IPacketSender packetSender = Resolve<IPacketSender>();

        // __state is a bool made to prevent duplicated entries, if it's false, then it should be skipped
        public static bool Prefix(StoryGoal goal, out bool __state)
        {
            bool skip = StoryGoalScheduler.main.schedule.Any(scheduledGoal => scheduledGoal.goalKey == goal.key);
            if (!skip && goal.goalType == Story.GoalType.Radio)
            {
                if (StoryGoalManager.main.pendingRadioMessages.Contains(goal.key))
                {
                    skip = true;
                }
            }
            __state = !skip;
            // if skip = false, it returns true : the function will be called normally
            // if skip = true,  it returns false: the function will be skipped
            return !skip;
        }


        public static void Postfix(StoryGoal goal, bool __state)
        {
            if (!__state || goal.key == "PlayerDiving" || PDALog.entries.ContainsKey(goal.key))
            {
                return;
            }

            ScheduledGoal scheduledGoal = StoryGoalScheduler.main.schedule.Find(scheduledGoal => scheduledGoal.goalKey == goal.key);
            if (scheduledGoal.timeExecute < DayNightCycle.main.timePassedAsDouble)
            {
                return;
            }
            
            packetSender.Send(new Schedule(scheduledGoal.timeExecute, goal.key, goal.goalType.ToString()));
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}

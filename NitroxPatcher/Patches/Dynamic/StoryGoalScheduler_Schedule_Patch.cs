using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxModel.Core;
using NitroxModel.Packets;
using Story;

namespace NitroxPatcher.Patches.Dynamic
{
    public class StoryGoalScheduler_Schedule_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(StoryGoalScheduler);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Schedule", BindingFlags.Public | BindingFlags.Instance);
        private static Dictionary<string, PDALog.Entry> entries = (Dictionary<string, PDALog.Entry>)(typeof(PDALog).GetField("entries", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));
        private static readonly IPacketSender iPacketSender = NitroxServiceLocator.LocateService<IPacketSender>();

        // __state is a bool made to prevent duplicated entries
        // if it's false, then it should be skipped
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
            // Log.Debug($"Prefix({goal.key}) [skip={skip}]");
            __state = !skip;
            // if skip = false, it returns true : the function will be called normally
            // if skip = true,  it returns false: the function will be skipped
            return !skip;
        }


        public static void Postfix(StoryGoal goal, bool __state)
        {
            if (!__state || goal.key == "PlayerDiving" || entries.TryGetValue(goal.key, out PDALog.Entry value))
            {
                return;
            }

            ScheduledGoal scheduledGoal = StoryGoalScheduler.main.schedule.Find(scheduledGoal => scheduledGoal.goalKey == goal.key);
            if (scheduledGoal.timeExecute < DayNightCycle.main.timePassedAsDouble)
            {
                return;
            }
            // Log.Debug($"StoryGoalScheduler.Schedule({scheduledGoal.goalKey};{scheduledGoal.goalType};{scheduledGoal.timeExecute})");
            iPacketSender.Send(new Schedule(scheduledGoal.timeExecute, goal.key, goal.goalType.ToString()));
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}

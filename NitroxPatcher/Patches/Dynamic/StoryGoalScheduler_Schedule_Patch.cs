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
            __state = StoryGoalScheduler.main.schedule.Any(scheduledGoal => scheduledGoal.goalKey == goal.key) ||
                      goal.goalType == Story.GoalType.Radio && StoryGoalManager.main.pendingRadioMessages.Contains(goal.key) ||
                      PDALog.entries.ContainsKey(goal.key);

            return !__state;
        }


        public static void Postfix(StoryGoal goal, bool __state)
        {
            if (__state || goal.key == "PlayerDiving")
            {
                return;
            }

            float timePassed = DayNightCycle.main ? ((float)DayNightCycle.main.timePassed) : 0f;
            packetSender.Send(new Schedule(timePassed + goal.delay, goal.key, goal.goalType.ToString()));
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}

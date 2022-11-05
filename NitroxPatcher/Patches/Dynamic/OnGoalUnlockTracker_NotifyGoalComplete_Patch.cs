using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxModel.Helper;
using NitroxModel.Packets;
using Story;

namespace NitroxPatcher.Patches.Dynamic
{
    public class OnGoalUnlockTracker_NotifyGoalComplete_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((OnGoalUnlockTracker t) => t.NotifyGoalComplete(default(string)));

        public static void Prefix(OnGoalUnlockTracker __instance, string completedGoal)
        {
            if (__instance.goalUnlocks.ContainsKey(completedGoal))
            {
                SendPacket<StoryEventSend>(new(StoryEventSend.EventType.GOAL_UNLOCK, completedGoal));
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
